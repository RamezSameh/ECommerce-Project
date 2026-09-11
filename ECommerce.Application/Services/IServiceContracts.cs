using ECommerce.Application.DTOs;

namespace ECommerce.Application.Services;

public interface IAuthService
{
    Task<AuthResponse> Register(RegisterDto registerDto);
    Task<AuthResponse> Login(LoginDto loginDto);
    Task<AuthResponse> Refresh(RefreshTokenDto dto);
    Task<AuthResponse> VerifyEmail(VerifyEmailDto dto);
    Task<AuthResponse> ForgotPassword(ForgotPasswordDto dto);
    Task<AuthResponse> ResetPassword(ResetPasswordDto dto);
    Task<AuthResponse> ChangePassword(string userId, ChangePasswordDto dto);
    Task<UserProfileDto> GetProfile(string userId);
    Task<UserProfileDto> UpdateProfile(string userId, UpdateProfileDto dto);
    Task Logout(string userId, string refreshToken);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(Core.Entities.User user, IList<string> roles, string jwtId);
    string GenerateRefreshToken(string userId, string jwtId);
    bool ValidateRefreshToken(string token, out Core.Entities.RefreshToken refreshToken);
}

public interface IEmailService
{
    /// <summary>Queues/sends an email. For the dev provider it just logs the content.</summary>
    Task SendAsync(string to, string subject, string htmlBody);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
    Task RemoveAsync(string key);
}

public interface IImageStorage
{
    Task<string> SaveAsync(Stream stream, string fileName, string folder);
    Task DeleteAsync(string url);
}