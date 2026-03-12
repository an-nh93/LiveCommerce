using LiveCommerce.Application.LiveSessions;
using LiveCommerce.Domain.Entities;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class LiveSessionService : ILiveSessionService
{
    private readonly AppDbContext _db;

    public LiveSessionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LiveSessionDto>> GetActiveByShopAsync(long shopId, CancellationToken ct = default)
    {
        return await _db.LiveSessions
            .AsNoTracking()
            .Where(s => s.ShopId == shopId && s.IsActive && s.EndedAtUtc == null)
            .OrderByDescending(s => s.StartedAtUtc)
            .Select(s => new LiveSessionDto
            {
                Id = s.Id,
                Name = s.Name,
                ExternalLiveId = s.ExternalLiveId,
                StartedAtUtc = s.StartedAtUtc,
                EndedAtUtc = s.EndedAtUtc,
                IsActive = s.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<LiveSessionDto?> GetByIdAsync(long id, long shopId, CancellationToken ct = default)
    {
        var s = await _db.LiveSessions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId, ct);
        return s == null ? null : new LiveSessionDto
        {
            Id = s.Id,
            Name = s.Name,
            ExternalLiveId = s.ExternalLiveId,
            StartedAtUtc = s.StartedAtUtc,
            EndedAtUtc = s.EndedAtUtc,
            IsActive = s.IsActive
        };
    }

    public async Task<LiveSessionDto?> CreateAsync(long shopId, CreateLiveSessionRequest request, CancellationToken ct = default)
    {
        var channel = await _db.ChannelConnections.FirstOrDefaultAsync(c => c.Id == request.ChannelConnectionId && c.ShopId == shopId, ct);
        if (channel == null) return null;

        var session = new LiveSession
        {
            ShopId = shopId,
            ChannelConnectionId = request.ChannelConnectionId,
            Name = request.Name,
            ExternalLiveId = request.ExternalLiveId,
            StartedAtUtc = DateTime.UtcNow,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.LiveSessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return new LiveSessionDto
        {
            Id = session.Id,
            Name = session.Name,
            ExternalLiveId = session.ExternalLiveId,
            StartedAtUtc = session.StartedAtUtc,
            EndedAtUtc = null,
            IsActive = true
        };
    }
}
