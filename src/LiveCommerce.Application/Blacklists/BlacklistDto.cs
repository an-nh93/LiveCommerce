using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Blacklists;

public class BlacklistDto
{
    public long Id { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Name { get; set; }
    public RiskLevel Level { get; set; }
    public string? Reason { get; set; }
}
