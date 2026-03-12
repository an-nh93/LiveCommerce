using LiveCommerce.Application.Dashboard;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LiveSummaryDto>> GetLiveSummaryAsync(long shopId, long? liveSessionId, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var from = fromUtc ?? DateTime.UtcNow.Date;
        var to = (toUtc ?? DateTime.UtcNow).AddDays(1);
        var sessionsQuery = _db.LiveSessions.AsNoTracking().Where(s => s.ShopId == shopId && s.StartedAtUtc >= from && s.StartedAtUtc < to);
        if (liveSessionId.HasValue) sessionsQuery = sessionsQuery.Where(s => s.Id == liveSessionId);
        var sessions = await sessionsQuery.ToListAsync(ct);
        var result = new List<LiveSummaryDto>();
        foreach (var s in sessions)
        {
            var comments = await _db.Comments.Where(c => c.LiveSessionId == s.Id).ToListAsync(ct);
            var orders = await _db.Orders.Where(o => o.LiveSessionId == s.Id).ToListAsync(ct);
            result.Add(new LiveSummaryDto
            {
                LiveSessionId = s.Id,
                LiveSessionName = s.Name,
                CommentCount = comments.Count,
                NewCount = comments.Count(c => c.Status == Domain.Enums.CommentStatus.New),
                OrderedCount = comments.Count(c => c.Status == Domain.Enums.CommentStatus.Ordered),
                OrderCount = orders.Count,
                EstimatedRevenue = orders.Sum(o => o.TotalAmount)
            });
        }
        return result;
    }

    public async Task<List<UserPerformanceDto>> GetUserPerformanceAsync(long shopId, long? liveSessionId, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var from = fromUtc ?? DateTime.UtcNow.Date;
        var to = (toUtc ?? DateTime.UtcNow).AddDays(1);
        var ordersQuery = _db.Orders.AsNoTracking().Where(o => o.ShopId == shopId && o.CreatedAtUtc >= from && o.CreatedAtUtc < to);
        if (liveSessionId.HasValue) ordersQuery = ordersQuery.Where(o => o.LiveSessionId == liveSessionId);
        var orders = await ordersQuery.Include(o => o.AssignedUser).ToListAsync(ct);
        var commentsQuery = _db.Comments.AsNoTracking().Where(c => c.ShopId == shopId && c.CommentTimeUtc >= from && c.CommentTimeUtc < to && c.AssignedUserId != null);
        if (liveSessionId.HasValue) commentsQuery = commentsQuery.Where(c => c.LiveSessionId == liveSessionId);
        var comments = await commentsQuery.ToListAsync(ct);
        var userIds = orders.Select(o => o.AssignedUserId)
            .Union(comments.Where(c => c.AssignedUserId != null).Select(c => c.AssignedUserId!.Value)).Distinct().ToList();
        var users = await _db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToListAsync(ct);
        return users.Select(u => new UserPerformanceDto
        {
            UserId = u.Id,
            UserName = u.DisplayName ?? u.Username,
            CommentsHandled = comments.Count(c => c.AssignedUserId == u.Id),
            OrdersCreated = orders.Count(o => o.AssignedUserId == u.Id),
            OrderRevenue = orders.Where(o => o.AssignedUserId == u.Id).Sum(o => o.TotalAmount)
        }).ToList();
    }

    public async Task<List<TopProductDto>> GetTopProductsAsync(long shopId, long? liveSessionId, int top, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default)
    {
        var from = fromUtc ?? DateTime.UtcNow.Date;
        var to = (toUtc ?? DateTime.UtcNow).AddDays(1);
        var orderIds = await _db.Orders.AsNoTracking()
            .Where(o => o.ShopId == shopId && o.CreatedAtUtc >= from && o.CreatedAtUtc < to)
            .Where(o => !liveSessionId.HasValue || o.LiveSessionId == liveSessionId)
            .Select(o => o.Id).ToListAsync(ct);
        var items = await _db.OrderItems.AsNoTracking()
            .Where(i => orderIds.Contains(i.OrderId))
            .Include(i => i.ProductVariant!).ThenInclude(v => v.Product)
            .ToListAsync(ct);
        var grouped = items.Where(i => i.ProductVariant != null).GroupBy(i => i.ProductVariant!.ProductId).Select(g => new TopProductDto
        {
            ProductId = g.Key,
            ProductName = g.First().ProductVariant!.Product?.Name ?? "",
            QuantitySold = g.Sum(x => x.Quantity),
            Revenue = g.Sum(x => x.LineTotal)
        }).OrderByDescending(x => x.QuantitySold).Take(top).ToList();
        return grouped;
    }
}
