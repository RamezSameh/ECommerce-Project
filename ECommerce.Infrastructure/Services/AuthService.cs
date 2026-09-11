using ECommerce.Application.DTOs;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ECommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _context;

    public AuthService(UserManager<User> userManager, ITokenService tokenService,
        IEmailService emailService, AppDbContext context)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _context = context;
    }

    public async Task<AuthResponse> Register(RegisterDto registerDto)
    {
        var existing = await _userManager.FindByEmailAsync(registerDto.Email);
        if (existing != null)
            throw new BusinessException("Email already exists");

        var user = new User
        {
            UserName = registerDto.UserName,
            Email = registerDto.Email,
            FullName = string.IsNullOrWhiteSpace(registerDto.FullName) ? registerDto.UserName : registerDto.FullName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // New registrations always start with the User role.
        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            throw new BadRequestException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));

        // Email verification
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(emailToken));
        var link = $"https://localhost:7294/api/auth/verify-email?email={user.Email}&token={encoded}";
        await _emailService.SendAsync(user.Email!, "Confirm your email",
            $"<a href=\"{link}\">Click to confirm your email</a>");

        return new AuthResponse
        {
            IsSuccess = true,
            Message = "User registered. Please confirm your email to enable login."
        };
    }

    public async Task<AuthResponse> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedException("Invalid email or password");

        if (user.IsBanned)
            throw new UnauthorizedException("Your account has been suspended");

        var roles = await _userManager.GetRolesAsync(user);
        return await BuildAuth(user, "Login successful", roles);
    }

    public async Task<AuthResponse> Refresh(RefreshTokenDto dto)
    {
        if (!_tokenService.ValidateRefreshToken(dto.RefreshToken, out var stored))
            throw new UnauthorizedException("Invalid or expired refresh token");

        var user = await _userManager.FindByIdAsync(stored.UserId);
        if (user == null || user.IsBanned)
            throw new UnauthorizedException("User no longer available");

        var roles = await _userManager.GetRolesAsync(user);
        await RevokeAllTokens(user);          // revoke all for this user
        return await BuildAuth(user, "Token refreshed", roles);
    }

    public async Task<AuthResponse> VerifyEmail(VerifyEmailDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new NotFoundException("User not found");

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await _userManager.ConfirmEmailAsync(user, decoded);
        if (!result.Succeeded)
            throw new BadRequestException("Invalid verification token");

        var roles = await _userManager.GetRolesAsync(user);
        await RevokeAllTokens(user);
        return await BuildAuth(user, "Email verified. You are now logged in", roles);
    }

    public async Task<AuthResponse> ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return new AuthResponse { IsSuccess = true, Message = "If the account exists, a reset link was sent." };

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        await _emailService.SendAsync(user.Email!, "Reset your password",
            $"Use this reset code: {encoded}");

        return new AuthResponse { IsSuccess = true, Message = "If the account exists, a reset link was sent." };
    }

    public async Task<AuthResponse> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new NotFoundException("User not found");

        var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
        var result = await _userManager.ResetPasswordAsync(user, decoded, dto.NewPassword);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        return new AuthResponse { IsSuccess = true, Message = "Password reset successfully" };
    }

    public async Task<AuthResponse> ChangePassword(string userId, ChangePasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            throw new NotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        // Revoke existing refresh tokens on password change for security.
        await RevokeAllTokens(user);
        return new AuthResponse { IsSuccess = true, Message = "Password changed. Please log in again." };
    }

    public async Task<UserProfileDto> GetProfile(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found");
        var roles = await _userManager.GetRolesAsync(user);
        return MapProfile(user, roles);
    }

    public async Task<UserProfileDto> UpdateProfile(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new NotFoundException("User not found");

        if (!string.IsNullOrWhiteSpace(dto.FullName)) user.FullName = dto.FullName;
        if (dto.Address != null) user.Address = dto.Address;
        if (dto.Phone != null)
        {
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, dto.Phone);
            if (!phoneResult.Succeeded)
                throw new BadRequestException(string.Join(", ", phoneResult.Errors.Select(e => e.Description)));
        }

        await _userManager.UpdateAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        return MapProfile(user, roles);
    }

    public async Task Logout(string userId, string refreshToken)
    {
        var hashed = TokenHelper.Hash(refreshToken);
        var stored = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.Token == hashed)
            .ToListAsync();
        foreach (var token in stored)
        {
            token.IsRevoked = true;
            _context.RefreshTokens.Update(token);
        }
        await _context.SaveChangesAsync();
    }

    // ---- helpers ----

    private async Task<AuthResponse> BuildAuth(User user, string message, IList<string> roles)
    {
        var (token, refreshToken, expiresAt) = await BuildPairAsync(user, roles);
        return new AuthResponse
        {
            IsSuccess = true,
            Message = message,
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            Email = user.Email,
            UserId = user.Id,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles.ToList()
        };
    }

    private async Task<(string Token, string RefreshToken, DateTime ExpiresAt)> BuildPairAsync(User user, IList<string> roles)
    {
        var jwtId = Guid.NewGuid().ToString();
        var (token, expiresAt) = _tokenService.GenerateToken(user, roles, jwtId);
        var refresh = _tokenService.GenerateRefreshToken(user.Id, jwtId);
        return (token, refresh, expiresAt);
    }

    private async Task RevokeAllTokens(User user)
    {
        var tokens = await _context.RefreshTokens.Where(t => t.UserId == user.Id).ToListAsync();
        foreach (var t in tokens) t.IsRevoked = true;
        await _context.SaveChangesAsync();
    }

    private static UserProfileDto MapProfile(User user, IList<string> roles) => new()
    {
        Id = user.Id,
        UserName = user.UserName,
        Email = user.Email,
        FullName = user.FullName,
        Address = user.Address,
        Phone = user.PhoneNumber,
        EmailConfirmed = user.EmailConfirmed
    };
}

public static class TokenHelper
{
    public static string Hash(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}