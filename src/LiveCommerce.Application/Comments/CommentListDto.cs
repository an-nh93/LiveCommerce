using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Comments;

public class CommentListDto
{
    public long Id { get; set; }
    public long LiveSessionId { get; set; }
    public string LiveSessionName { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CommentTimeUtc { get; set; }
    public string? SenderName { get; set; }
    public string? SenderExternalId { get; set; }
    public CommentStatus Status { get; set; }
    public long? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public bool IsSpam { get; set; }
}
