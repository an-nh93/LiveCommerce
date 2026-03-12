using LiveCommerce.Domain.Entities;
using LiveCommerce.Domain.Enums;
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

        var channel = new ChannelConnection
        {
            ShopId = shop.Id,
            ChannelType = ChannelType.TikTok,
            ChannelName = "TikTok Demo",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.ChannelConnections.Add(channel);
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

        var liveSession = new LiveSession
        {
            ShopId = shop.Id,
            ChannelConnectionId = channel.Id,
            Name = "Livestream demo",
            ExternalLiveId = "demo-live-1",
            StartedAtUtc = DateTime.UtcNow,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.LiveSessions.Add(liveSession);
        await db.SaveChangesAsync(ct);

        var product = new Product
        {
            ShopId = shop.Id,
            Code = "SP01",
            Name = "Sản phẩm mẫu",
            BasePrice = 100000,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        db.ProductVariants.Add(new ProductVariant
        {
            ProductId = product.Id,
            Sku = "SP01-DEF",
            Price = 100000,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Seed data created: Shop SHOP01, user admin/admin123, channel, live session, product");
    }
}
