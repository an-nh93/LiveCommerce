using LiveCommerce.Application.Blacklists;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Domain.Entities;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class BlacklistService : IBlacklistService
{
    private readonly AppDbContext _db;

    public BlacklistService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BlacklistDto>> GetByShopAsync(long shopId, CancellationToken ct = default)
    {
        return await _db.Blacklists.AsNoTracking()
            .Where(b => b.ShopId == shopId)
            .OrderBy(b => b.Phone)
            .Select(b => new BlacklistDto
            {
                Id = b.Id,
                Phone = b.Phone,
                Address = b.Address,
                Name = b.Name,
                Level = b.Level,
                Reason = b.Reason
            })
            .ToListAsync(ct);
    }

    public async Task<BlacklistDto?> CreateAsync(long shopId, long userId, CreateBlacklistRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Phone) && string.IsNullOrWhiteSpace(request.Address) && string.IsNullOrWhiteSpace(request.Name))
            return null;
        var b = new Blacklist
        {
            ShopId = shopId,
            CreatedByUserId = userId,
            Phone = request.Phone?.Trim(),
            Address = request.Address?.Trim(),
            Name = request.Name?.Trim(),
            Level = (RiskLevel)request.Level,
            Reason = request.Reason?.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Blacklists.Add(b);
        await _db.SaveChangesAsync(ct);
        return new BlacklistDto { Id = b.Id, Phone = b.Phone, Address = b.Address, Name = b.Name, Level = b.Level, Reason = b.Reason };
    }

    public async Task<bool> IsBlacklistedAsync(long shopId, string? phone, string? address, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(address)) return false;
        var q = _db.Blacklists.AsNoTracking().Where(b => b.ShopId == shopId);
        if (!string.IsNullOrWhiteSpace(phone))
            q = q.Where(b => b.Phone != null && b.Phone == phone.Trim());
        else if (!string.IsNullOrWhiteSpace(address))
            q = q.Where(b => b.Address != null && b.Address.Contains(address.Trim()));
        else return false;
        return await q.AnyAsync(ct);
    }
}
