using LiveCommerce.Application.FollowUps;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Domain.Entities;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class FollowUpService : IFollowUpService
{
    private readonly AppDbContext _db;

    public FollowUpService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<FollowUpListDto>> GetByShopAsync(long shopId, bool pendingOnly, CancellationToken ct = default)
    {
        var q = _db.FollowUps.AsNoTracking().Include(f => f.AssignedUser).Where(f => f.ShopId == shopId);
        if (pendingOnly) q = q.Where(f => f.Status == FollowUpStatus.Pending || f.Status == FollowUpStatus.InProgress);
        return await q.OrderBy(f => f.TargetTimeUtc)
            .Select(f => new FollowUpListDto
            {
                Id = f.Id,
                TargetTimeUtc = f.TargetTimeUtc,
                Status = (int)f.Status,
                Note = f.Note,
                AssignedUserName = f.AssignedUser != null ? f.AssignedUser.DisplayName ?? f.AssignedUser.Username : null,
                CommentId = f.CommentId,
                CustomerId = f.CustomerId
            })
            .ToListAsync(ct);
    }

    public async Task<FollowUpListDto?> CreateAsync(long shopId, long userId, CreateFollowUpRequest request, CancellationToken ct = default)
    {
        var f = new FollowUp
        {
            ShopId = shopId,
            CommentId = request.CommentId,
            CustomerId = request.CustomerId,
            AssignedUserId = request.AssignedUserId,
            TargetTimeUtc = request.TargetTimeUtc,
            Note = request.Note,
            Status = FollowUpStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.FollowUps.Add(f);
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(f.Id, ct);
    }

    public async Task<FollowUpListDto?> MarkDoneAsync(long id, long shopId, long userId, string? note, CancellationToken ct = default)
    {
        var f = await _db.FollowUps.Include(x => x.AssignedUser).FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId, ct);
        if (f == null) return null;
        f.Status = FollowUpStatus.Done;
        f.UpdatedAtUtc = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(note)) f.Note = (f.Note + " " + note).Trim();
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(id, ct);
    }

    private async Task<FollowUpListDto?> GetByIdAsync(long id, CancellationToken ct)
    {
        var f = await _db.FollowUps.AsNoTracking().Include(x => x.AssignedUser)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return f == null ? null : new FollowUpListDto
        {
            Id = f.Id,
            TargetTimeUtc = f.TargetTimeUtc,
            Status = (int)f.Status,
            Note = f.Note,
            AssignedUserName = f.AssignedUser != null ? f.AssignedUser.DisplayName ?? f.AssignedUser.Username : null,
            CommentId = f.CommentId,
            CustomerId = f.CustomerId
        };
    }
}
