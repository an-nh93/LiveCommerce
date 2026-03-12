using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Orders;

public class OrderDetailDto : OrderListDto
{
    public string? ReceiverAddress { get; set; }
    public string? Note { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public long? CommentId { get; set; }
    public long? CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public long ProductVariantId { get; set; }
    public string? ProductName { get; set; }
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
