using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class Comment : Common.ShopScopedEntity
{
    public long LiveSessionId { get; set; }
    public LiveSession LiveSession { get; set; } = null!;
    public long? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public long? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    public string ExternalCommentId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CommentTimeUtc { get; set; }
    public string? SenderExternalId { get; set; }
    public string? SenderName { get; set; }
    public string? RawPayloadJson { get; set; }

    public CommentStatus Status { get; set; } = CommentStatus.New;
    public bool IsSpam { get; set; }

    public Order? Order { get; set; }
    public ICollection<CommentLog> Logs { get; set; } = new List<CommentLog>();
}
