using LiveCommerce.Application.Comments;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class CommentAssignmentService : ICommentAssignmentService
{
    private readonly AppDbContext _db;

    public CommentAssignmentService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CommentListDto?> TakeAsync(long commentId, long userId, long shopId, CancellationToken ct = default)
    {
        var comment = await _db.Comments
            .Include(c => c.LiveSession)
            .Include(c => c.AssignedUser)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ShopId == shopId, ct);
        if (comment == null) return null;
        var fromStatus = comment.Status;
        if (fromStatus != CommentStatus.New && fromStatus != CommentStatus.Assigned)
            return null;
        comment.AssignedUserId = userId;
        comment.Status = CommentStatus.Assigned;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        _db.CommentLogs.Add(new Domain.Entities.CommentLog
        {
            CommentId = comment.Id,
            ActorUserId = userId,
            FromStatus = fromStatus,
            ToStatus = CommentStatus.Assigned,
            Note = "Take",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return MapToListDto(comment);
    }

    public async Task<CommentListDto?> AssignAsync(long commentId, long assignToUserId, long actorUserId, long shopId, CancellationToken ct = default)
    {
        var comment = await _db.Comments
            .Include(c => c.LiveSession)
            .Include(c => c.AssignedUser)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ShopId == shopId, ct);
        if (comment == null) return null;
        var fromStatus = comment.Status;
        comment.AssignedUserId = assignToUserId;
        comment.Status = CommentStatus.Assigned;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        _db.CommentLogs.Add(new Domain.Entities.CommentLog
        {
            CommentId = comment.Id,
            ActorUserId = actorUserId,
            FromStatus = fromStatus,
            ToStatus = CommentStatus.Assigned,
            Note = "Assign",
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return MapToListDto(comment);
    }

    public async Task<CommentListDto?> UpdateStatusAsync(long commentId, CommentStatus toStatus, long actorUserId, long shopId, string? note = null, CancellationToken ct = default)
    {
        var comment = await _db.Comments
            .Include(c => c.LiveSession)
            .Include(c => c.AssignedUser)
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ShopId == shopId, ct);
        if (comment == null) return null;
        var fromStatus = comment.Status;
        if (!IsValidTransition(fromStatus, toStatus)) return null;
        comment.Status = toStatus;
        comment.UpdatedAtUtc = DateTime.UtcNow;
        _db.CommentLogs.Add(new Domain.Entities.CommentLog
        {
            CommentId = comment.Id,
            ActorUserId = actorUserId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            Note = note,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return MapToListDto(comment);
    }

    public async Task<CommentDetailDto?> GetByIdAsync(long commentId, long shopId, CancellationToken ct = default)
    {
        var comment = await _db.Comments
            .Include(c => c.LiveSession)
            .Include(c => c.AssignedUser)
            .Include(c => c.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == commentId && c.ShopId == shopId, ct);
        if (comment == null) return null;
        return new CommentDetailDto
        {
            Id = comment.Id,
            LiveSessionId = comment.LiveSessionId,
            LiveSessionName = comment.LiveSession.Name,
            Content = comment.Content,
            CommentTimeUtc = comment.CommentTimeUtc,
            SenderName = comment.SenderName,
            SenderExternalId = comment.SenderExternalId,
            Status = comment.Status,
            AssignedUserId = comment.AssignedUserId,
            AssignedUserName = comment.AssignedUser != null ? comment.AssignedUser.DisplayName ?? comment.AssignedUser.Username : null,
            IsSpam = comment.IsSpam,
            RawPayloadJson = comment.RawPayloadJson,
            CustomerId = comment.CustomerId,
            CustomerName = comment.Customer?.Name,
            CustomerPhone = comment.Customer?.Phone
        };
    }

    private static bool IsValidTransition(CommentStatus from, CommentStatus to)
    {
        if (from == to) return true;
        return (from, to) switch
        {
            (CommentStatus.New, _) => true,
            (CommentStatus.Assigned, CommentStatus.InProgress) or (CommentStatus.Assigned, CommentStatus.Ignored) or (CommentStatus.Assigned, CommentStatus.Cancelled) => true,
            (CommentStatus.InProgress, CommentStatus.Ordered) or (CommentStatus.InProgress, CommentStatus.NeedFollowUp) or (CommentStatus.InProgress, CommentStatus.Ignored) or (CommentStatus.InProgress, CommentStatus.Cancelled) => true,
            _ => false
        };
    }

    private static CommentListDto MapToListDto(Domain.Entities.Comment c)
    {
        return new CommentListDto
        {
            Id = c.Id,
            LiveSessionId = c.LiveSessionId,
            LiveSessionName = c.LiveSession.Name,
            Content = c.Content,
            CommentTimeUtc = c.CommentTimeUtc,
            SenderName = c.SenderName,
            SenderExternalId = c.SenderExternalId,
            Status = c.Status,
            AssignedUserId = c.AssignedUserId,
            AssignedUserName = c.AssignedUser != null ? c.AssignedUser.DisplayName ?? c.AssignedUser.Username : null,
            IsSpam = c.IsSpam
        };
    }
}
