using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Comments;

public interface ICommentAssignmentService
{
    Task<CommentListDto?> TakeAsync(long commentId, long userId, long shopId, CancellationToken ct = default);
    Task<CommentListDto?> AssignAsync(long commentId, long assignToUserId, long actorUserId, long shopId, CancellationToken ct = default);
    Task<CommentListDto?> UpdateStatusAsync(long commentId, CommentStatus toStatus, long actorUserId, long shopId, string? note = null, CancellationToken ct = default);
    Task<CommentDetailDto?> GetByIdAsync(long commentId, long shopId, CancellationToken ct = default);
}

public class CommentDetailDto : CommentListDto
{
    public string? RawPayloadJson { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
}
