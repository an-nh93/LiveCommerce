using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Domain.Entities;

public class CommentLog : Common.BaseEntity
{
    public long CommentId { get; set; }
    public Comment Comment { get; set; } = null!;
    public long ActorUserId { get; set; }
    public User ActorUser { get; set; } = null!;
    public CommentStatus FromStatus { get; set; }
    public CommentStatus ToStatus { get; set; }
    public string? Note { get; set; }
}
