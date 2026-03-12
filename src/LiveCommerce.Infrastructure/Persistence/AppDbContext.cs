using LiveCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Shop> Shops => Set<Shop>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<ChannelConnection> ChannelConnections => Set<ChannelConnection>();
    public DbSet<LiveSession> LiveSessions => Set<LiveSession>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerTag> CustomerTags => Set<CustomerTag>();
    public DbSet<CustomerTagMapping> CustomerTagMappings => Set<CustomerTagMapping>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderLog> OrderLogs => Set<OrderLog>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentLog> CommentLogs => Set<CommentLog>();
    public DbSet<FollowUp> FollowUps => Set<FollowUp>();
    public DbSet<Blacklist> Blacklists => Set<Blacklist>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // RolePermission composite key
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });
        modelBuilder.Entity<CustomerTagMapping>()
            .HasKey(ctm => new { ctm.CustomerId, ctm.CustomerTagId });

        // Indexes per PRD
        modelBuilder.Entity<Comment>()
            .HasIndex(c => new { c.ShopId, c.LiveSessionId, c.Status, c.CommentTimeUtc });
        modelBuilder.Entity<Comment>()
            .HasIndex(c => new { c.AssignedUserId, c.Status });
        modelBuilder.Entity<Order>()
            .HasIndex(o => new { o.ShopId, o.Status, o.CreatedAtUtc });
        modelBuilder.Entity<Customer>()
            .HasIndex(c => new { c.ShopId, c.Phone });
        modelBuilder.Entity<Customer>()
            .HasIndex(c => c.ExternalCustomerId);
    }
}
