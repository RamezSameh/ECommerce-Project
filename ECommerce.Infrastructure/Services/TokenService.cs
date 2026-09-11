using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ECommerce.Application.Services;
using ECommerce.Core.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwt;
    private readonly AppDbContext _context;

    public TokenService(IOptions<JwtSettings> jwt, AppDbContext context)
    {
        _jwt = jwt.Value;
        _context = context;
    }

    /// <summary>Mints a signed JWT access token. The returned jti is embedded for refresh-token pairing.</summary>
    public (string Token, DateTime ExpiresAt) GenerateToken(User user, IList<string> roles, string jwtId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!),
            new(JwtRegisteredClaimNames.Jti, jwtId),
            new(ClaimTypes.NameIdentifier, user.Id)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    /// <summary>Creates a random opaque refresh token and persists a hash alongside the owning jwt id.</summary>
    public string GenerateRefreshToken(string userId, string jwtId)
    {
        var token = GenerateOpaqueToken();
        var hashed = HashToken(token);

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = userId,
            Token = hashed,
            JwtId = jwtId,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        });
        _context.SaveChanges();

        return token;
    }

    /// <summary>
    /// Validates a refresh token (exists, not used, not revoked, not expired).
    /// Marks it as used to enforce rotation.
    /// </summary>
    public bool ValidateRefreshToken(string token, out RefreshToken refreshToken)
    {
        refreshToken = null!;
        var hashed = HashToken(token);
        var stored = _context.RefreshTokens.SingleOrDefault(rt => rt.Token == hashed);

        if (stored is null || stored.IsExpired || stored.IsUsed || stored.IsRevoked)
            return false;

        stored.IsUsed = true;   // rotation: the same token cannot be reused
        _context.Update(stored);
        _context.SaveChanges();

        refreshToken = stored;
        return true;
    }

    private static string GenerateOpaqueToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashToken(string token)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(token)));
    }
}