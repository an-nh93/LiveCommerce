using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class Blacklist : Common.ShopScopedEntity
{
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Name { get; set; }
    public RiskLevel Level { get; set; } = RiskLevel.High;
    public string? Reason { get; set; }
    public long CreatedByUserId { get; set; }
}
