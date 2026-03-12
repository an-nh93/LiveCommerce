using LiveCommerce.Application.Users;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserListDto>> GetByShopAsync(long shopId, CancellationToken ct = default)
    {
        return await _db.Users.AsNoTracking()
            .Where(u => u.ShopId == shopId)
            .Include(u => u.Role)
            .OrderBy(u => u.Username)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                RoleCode = u.Role.Code,
                IsActive = u.IsActive
            })
            .ToListAsync(ct);
    }
}
