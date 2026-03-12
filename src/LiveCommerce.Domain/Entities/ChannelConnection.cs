using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class ChannelConnection : Common.ShopScopedEntity
{
    public ChannelType ChannelType { get; set; }
    public string ChannelName { get; set; } = null!;
    public string? ExternalPageId { get; set; }
    public string? ExternalAccountId { get; set; }
    public string? TokenOrConfigJson { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? TokenExpiryUtc { get; set; }
    public DateTime? LastValidatedUtc { get; set; }
}
