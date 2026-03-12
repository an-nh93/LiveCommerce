using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class Customer : Common.ShopScopedEntity
{
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public RiskLevel RiskLevel { get; set; } = RiskLevel.None;
    public string? ExternalCustomerId { get; set; }
    public string? ChannelIdentifiersJson { get; set; }

    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderDateUtc { get; set; }

    public ICollection<CustomerTagMapping> TagMappings { get; set; } = new List<CustomerTagMapping>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
