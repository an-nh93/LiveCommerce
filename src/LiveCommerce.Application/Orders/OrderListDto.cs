using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Orders;

public class OrderListDto
{
    public long Id { get; set; }
    public string OrderNo { get; set; } = null!;
    public long? LiveSessionId { get; set; }
    public string? ReceiverName { get; set; }
    public string? ReceiverPhone { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
