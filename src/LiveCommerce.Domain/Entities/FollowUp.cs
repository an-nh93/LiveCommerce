using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class FollowUp : Common.ShopScopedEntity
{
    public long? CommentId { get; set; }
    public Comment? Comment { get; set; }
    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public long AssignedUserId { get; set; }
    public User AssignedUser { get; set; } = null!;

    public DateTime TargetTimeUtc { get; set; }
    public FollowUpStatus Status { get; set; } = FollowUpStatus.Pending;
    public string? Note { get; set; }
}
