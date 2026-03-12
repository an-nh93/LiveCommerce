namespace LiveCommerce.Domain.Entities;

public class CustomerTagMapping
{
    public long CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public long CustomerTagId { get; set; }
    public CustomerTag CustomerTag { get; set; } = null!;
    public DateTime AssignedAtUtc { get; set; }
}
