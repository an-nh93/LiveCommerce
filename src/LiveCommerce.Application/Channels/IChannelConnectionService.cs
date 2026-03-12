namespace LiveCommerce.Application.Channels;

public interface IChannelConnectionService
{
    Task<List<ChannelConnectionDto>> GetByShopAsync(long shopId, CancellationToken ct = default);
    Task<ChannelConnectionDto?> CreateAsync(long shopId, CreateChannelRequest request, CancellationToken ct = default);
}

public class CreateChannelRequest
{
    public int ChannelType { get; set; }
    public string ChannelName { get; set; } = null!;
}
