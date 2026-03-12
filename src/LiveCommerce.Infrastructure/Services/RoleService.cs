using LiveCommerce.Application.Roles;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;

    public RoleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RoleListDto>> GetByShopAsync(CancellationToken ct = default)
    {
        return await _db.Roles.AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Code)
            .Select(r => new RoleListDto
            {
                Id = r.Id,
                Code = r.Code,
                Name = r.Name,
                PermissionCodes = r.RolePermissions.Select(rp => rp.Permission.Code).ToList()
            })
            .ToListAsync(ct);
    }
}
