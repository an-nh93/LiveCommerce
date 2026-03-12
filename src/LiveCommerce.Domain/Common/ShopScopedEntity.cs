using LiveCommerce.Domain.Entities;

namespace LiveCommerce.Domain.Common;

public abstract class ShopScopedEntity : BaseEntity
{
    public long ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
}
