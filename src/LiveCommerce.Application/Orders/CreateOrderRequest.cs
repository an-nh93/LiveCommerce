namespace LiveCommerce.Application.Orders;

public class CreateOrderRequest
{
    public long? LiveSessionId { get; set; }
    public long? CommentId { get; set; }
    public long? CustomerId { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public string? ReceiverAddress { get; set; }
    public string? Note { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Discount { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    public long ProductVariantId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
