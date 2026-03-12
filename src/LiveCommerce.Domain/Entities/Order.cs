using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class Order : Common.ShopScopedEntity
{
    public string OrderNo { get; set; } = null!;
    public long? LiveSessionId { get; set; }
    public LiveSession? LiveSession { get; set; }
    public long? CommentId { get; set; }
    public Comment? SourceComment { get; set; }
    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public long AssignedUserId { get; set; }
    public User AssignedUser { get; set; } = null!;

    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverAddress { get; set; }
    public string? Note { get; set; }

    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public RiskLevel? RiskLevel { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public ICollection<OrderLog> Logs { get; set; } = new List<OrderLog>();
}
