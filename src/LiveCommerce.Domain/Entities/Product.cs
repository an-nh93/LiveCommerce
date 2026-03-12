namespace LiveCommerce.Domain.Entities;

public class Product : Common.ShopScopedEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
