namespace LiveCommerce.Application.Products;

public class ProductListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Category { get; set; }
    public decimal BasePrice { get; set; }
    public bool IsActive { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();
}

public class ProductVariantDto
{
    public long Id { get; set; }
    public string Sku { get; set; } = null!;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal Price { get; set; }
}
