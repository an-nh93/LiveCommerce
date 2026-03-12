using LiveCommerce.Application.Comments;
using LiveCommerce.Infrastructure.Persistence;
using LiveCommerce.Shared;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class CommentQueryService : ICommentQueryService
{
    private readonly AppDbContext _db;

    public CommentQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<CommentListDto>> GetListAsync(long shopId, CommentFilterDto filter, CancellationToken ct = default)
    {
        var q = _db.Comments
            .AsNoTracking()
            .Where(c => c.ShopId == shopId)
            .Include(c => c.LiveSession)
            .Include(c => c.AssignedUser)
            .AsQueryable();

        if (filter.LiveSessionId.HasValue)
            q = q.Where(c => c.LiveSessionId == filter.LiveSessionId);
        if (filter.Status.HasValue)
            q = q.Where(c => c.Status == filter.Status);
        if (filter.AssignedUserId.HasValue)
            q = q.Where(c => c.AssignedUserId == filter.AssignedUserId);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(c => c.CommentTimeUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(c => new CommentListDto
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
            })
            .ToListAsync(ct);

        return new PagedResult<CommentListDto>
        {
            Items = items,
            TotalCount = total,
            Page = filter.Page,
            PageSize = filter.PageSize
        };
    }
}
