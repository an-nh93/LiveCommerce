namespace LiveCommerce.Domain.Entities;

public class CustomerTag : Common.ShopScopedEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ColorHex { get; set; }

    public ICollection<CustomerTagMapping> CustomerTagMappings { get; set; } = new List<CustomerTagMapping>();
}
