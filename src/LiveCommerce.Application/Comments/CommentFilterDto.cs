using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Comments;

public class CommentFilterDto
{
    public long? LiveSessionId { get; set; }
    public CommentStatus? Status { get; set; }
    public long? AssignedUserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
