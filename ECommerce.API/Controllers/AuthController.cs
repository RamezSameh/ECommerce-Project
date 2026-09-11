using ECommerce.Application.DTOs;
using ECommerce.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
        => Ok(ApiResponse<AuthResponse>.Success(await _authService.Register(dto)));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
        => Ok(ApiResponse<AuthResponse>.Success(await _authService.Login(dto)));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto)
        => Ok(ApiResponse<AuthResponse>.Success(await _authService.Refresh(dto)));

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailDto dto)
        => Ok(ApiResponse<AuthResponse>.Success(await _authService.VerifyEmail(dto)));

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
        => Ok(ApiResponse<AuthResponse>.Success(await _authService.ForgotPassword(dto)));

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        => Ok(ApiResponse<AuthResponse>.Success(await _authService.ResetPassword(dto)));

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(ApiResponse<AuthResponse>.Success(await _authService.ChangePassword(userId, dto)));
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _authService.Logout(userId, dto.RefreshToken);
        return Ok(ApiResponse<object>.Success(null!, "Logged out"));
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(ApiResponse<UserProfileDto>.Success(await _authService.GetProfile(userId)));
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(ApiResponse<UserProfileDto>.Success(await _authService.UpdateProfile(userId, dto)));
    }
}