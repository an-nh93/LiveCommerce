namespace LiveCommerce.Application.LiveSessions;

public interface ILiveSessionService
{
    Task<List<LiveSessionDto>> GetActiveByShopAsync(long shopId, CancellationToken ct = default);
    Task<LiveSessionDto?> GetByIdAsync(long id, long shopId, CancellationToken ct = default);
    Task<LiveSessionDto?> CreateAsync(long shopId, CreateLiveSessionRequest request, CancellationToken ct = default);
}

public class CreateLiveSessionRequest
{
    public string Name { get; set; } = null!;
    public long ChannelConnectionId { get; set; }
    public string? ExternalLiveId { get; set; }
}
