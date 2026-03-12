using LiveCommerce.Application.Channels;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Domain.Entities;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class ChannelConnectionService : IChannelConnectionService
{
    private readonly AppDbContext _db;

    public ChannelConnectionService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ChannelConnectionDto>> GetByShopAsync(long shopId, CancellationToken ct = default)
    {
        return await _db.ChannelConnections.AsNoTracking()
            .Where(c => c.ShopId == shopId)
            .OrderBy(c => c.ChannelName)
            .Select(c => new ChannelConnectionDto
            {
                Id = c.Id,
                ChannelType = c.ChannelType,
                ChannelName = c.ChannelName,
                IsActive = c.IsActive
            })
            .ToListAsync(ct);
    }

    public async Task<ChannelConnectionDto?> CreateAsync(long shopId, CreateChannelRequest request, CancellationToken ct = default)
    {
        var c = new ChannelConnection
        {
            ShopId = shopId,
            ChannelType = (ChannelType)request.ChannelType,
            ChannelName = request.ChannelName,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.ChannelConnections.Add(c);
        await _db.SaveChangesAsync(ct);
        return new ChannelConnectionDto { Id = c.Id, ChannelType = c.ChannelType, ChannelName = c.ChannelName, IsActive = c.IsActive };
    }
}
