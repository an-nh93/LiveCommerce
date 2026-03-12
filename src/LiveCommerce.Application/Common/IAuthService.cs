namespace LiveCommerce.Application.Common;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(string shopCode, string username, string password, CancellationToken ct = default);
    Task<LoginResult?> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<CurrentUserDto?> GetCurrentUserAsync(long userId, long shopId, CancellationToken ct = default);
}

public class LoginResult
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAtUtc { get; set; }
    public CurrentUserDto User { get; set; } = null!;
}

public class CurrentUserDto
{
    public long UserId { get; set; }
    public long ShopId { get; set; }
    public string ShopCode { get; set; } = null!;
    public string ShopName { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string RoleCode { get; set; } = null!;
    public List<string> Permissions { get; set; } = new();
}
