using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class OrderLog : Common.BaseEntity
{
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long ActorUserId { get; set; }
    public User ActorUser { get; set; } = null!;
    public OrderStatus FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Note { get; set; }
}
