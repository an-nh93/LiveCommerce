using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LiveCommerce.Application.Common;
using LiveCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LiveCommerce.Infrastructure.Persistence;

namespace LiveCommerce.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<LoginResult?> LoginAsync(string shopCode, string username, string password, CancellationToken ct = default)
    {
        var shop = await _db.Shops.AsNoTracking().FirstOrDefaultAsync(s => s.Code == shopCode && s.IsActive, ct);
        if (shop == null) return null;

        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.ShopId == shop.Id && u.Username == username && u.IsActive, ct);
        if (user == null) return null;

        if (!VerifyPassword(password, user.PasswordHash)) return null;

        var permissions = user.Role.RolePermissions.Select(rp => rp.Permission.Code).Distinct().ToList();
        var userDto = new CurrentUserDto
        {
            UserId = user.Id,
            ShopId = shop.Id,
            ShopCode = shop.Code,
            ShopName = shop.Name,
            Username = user.Username,
            DisplayName = user.DisplayName,
            RoleCode = user.Role.Code,
            Permissions = permissions
        };

        var accessToken = GenerateAccessToken(user.Id, shop.Id, user.Username, user.Role.Code, permissions);
        var refreshToken = GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes());

        return new LoginResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = expiresAt,
            User = userDto
        };
    }

    public async Task<LoginResult?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // MVP: simplified - in production store refresh tokens in DB/Redis and validate
        await Task.CompletedTask;
        return null;
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(long userId, long shopId, CancellationToken ct = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.Shop)
            .Include(u => u.Role)
            .ThenInclude(r => r!.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Id == userId && u.ShopId == shopId && u.IsActive, ct);
        if (user == null) return null;

        return new CurrentUserDto
        {
            UserId = user.Id,
            ShopId = user.ShopId,
            ShopCode = user.Shop.Code,
            ShopName = user.Shop.Name,
            Username = user.Username,
            DisplayName = user.DisplayName,
            RoleCode = user.Role.Code,
            Permissions = user.Role.RolePermissions.Select(rp => rp.Permission.Code).Distinct().ToList()
        };
    }

    private string GenerateAccessToken(long userId, long shopId, string username, string roleCode, List<string> permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtSecret()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new("shop_id", shopId.ToString()),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.Role, roleCode)
        };
        foreach (var p in permissions)
            claims.Add(new Claim("permission", p));

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "LiveCommerce",
            audience: _config["Jwt:Audience"] ?? "LiveCommerce",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(GetAccessTokenExpiryMinutes()),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        // MVP: BCrypt or PBKDF2 recommended; here simple hash for demo
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var computed = Convert.ToBase64String(sha.ComputeHash(bytes));
        return computed == hash;
    }

    public static string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }

    private string GetJwtSecret() => _config["Jwt:Key"] ?? "LiveCommerce-SuperSecret-Key-Min-32-Chars!!";
    private int GetAccessTokenExpiryMinutes() => int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 60;
}
