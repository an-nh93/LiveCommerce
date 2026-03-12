namespace LiveCommerce.Domain.Entities;

public class ProductVariant : Common.BaseEntity
{
    public long ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string Sku { get; set; } = null!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
