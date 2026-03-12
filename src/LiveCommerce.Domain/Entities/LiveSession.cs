namespace LiveCommerce.Domain.Entities;

public class LiveSession : Common.ShopScopedEntity
{
    public long ChannelConnectionId { get; set; }
    public ChannelConnection ChannelConnection { get; set; } = null!;

    public string Name { get; set; } = null!;
    public string? ExternalLiveId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
