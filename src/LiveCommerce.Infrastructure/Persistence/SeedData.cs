using LiveCommerce.Domain.Entities;
using LiveCommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LiveCommerce.Infrastructure.Persistence;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        if (await db.Shops.AnyAsync(ct)) return;

        var shop = new Shop
        {
            Code = "SHOP01",
            Name = "Demo Shop",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Shops.Add(shop);
        await db.SaveChangesAsync(ct);

        var role = new Role
        {
            Code = "ADMIN",
            Name = "Administrator",
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Roles.Add(role);
        await db.SaveChangesAsync(ct);

        var permComment = new Permission { Code = "comment.view", Name = "View comments", Module = "Comment", CreatedAtUtc = DateTime.UtcNow };
        var permOrder = new Permission { Code = "order.manage", Name = "Manage orders", Module = "Order", CreatedAtUtc = DateTime.UtcNow };
        var permDashboard = new Permission { Code = "dashboard.view", Name = "View dashboard", Module = "Dashboard", CreatedAtUtc = DateTime.UtcNow };
        db.Permissions.AddRange(permComment, permOrder, permDashboard);
        await db.SaveChangesAsync(ct);

        db.RolePermissions.AddRange(
            new RolePermission { RoleId = role.Id, PermissionId = permComment.Id },
            new RolePermission { RoleId = role.Id, PermissionId = permOrder.Id },
            new RolePermission { RoleId = role.Id, PermissionId = permDashboard.Id });
        await db.SaveChangesAsync(ct);

        var user = new User
        {
            ShopId = shop.Id,
            RoleId = role.Id,
            Username = "admin",
            PasswordHash = AuthService.HashPassword("admin123"),
            DisplayName = "Admin",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seed data created: Shop SHOP01, user admin / admin123");
    }
}
