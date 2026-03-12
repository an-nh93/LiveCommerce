namespace LiveCommerce.Domain.Entities;

public class OrderItem : Common.BaseEntity
{
    public long OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public long ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
