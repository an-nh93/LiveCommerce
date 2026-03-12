namespace LiveCommerce.Application.Blacklists;

public interface IBlacklistService
{
    Task<List<BlacklistDto>> GetByShopAsync(long shopId, CancellationToken ct = default);
    Task<BlacklistDto?> CreateAsync(long shopId, long userId, CreateBlacklistRequest request, CancellationToken ct = default);
    Task<bool> IsBlacklistedAsync(long shopId, string? phone, string? address, CancellationToken ct = default);
}

public class CreateBlacklistRequest
{
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Name { get; set; }
    public int Level { get; set; } = 3; // High
    public string? Reason { get; set; }
}
