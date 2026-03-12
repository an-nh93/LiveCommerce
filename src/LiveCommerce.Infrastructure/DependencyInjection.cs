using LiveCommerce.Application.Common;
using LiveCommerce.Application.Comments;
using LiveCommerce.Application.LiveSessions;
using LiveCommerce.Application.Blacklists;
using LiveCommerce.Application.Channels;
using LiveCommerce.Application.Dashboard;
using LiveCommerce.Application.FollowUps;
using LiveCommerce.Application.Orders;
using LiveCommerce.Application.Products;
using LiveCommerce.Application.Roles;
using LiveCommerce.Application.Users;
using LiveCommerce.Infrastructure.Messaging;
using LiveCommerce.Infrastructure.Persistence;
using LiveCommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LiveCommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitSection = configuration.GetSection(RabbitMQOptions.Section);
        services.Configure<RabbitMQOptions>(opt =>
        {
            opt.HostName = rabbitSection["HostName"] ?? "localhost";
            opt.Port = int.TryParse(rabbitSection["Port"], out var p) ? p : 5672;
            opt.UserName = rabbitSection["UserName"] ?? "guest";
            opt.Password = rabbitSection["Password"] ?? "guest";
            opt.CommentIngestionQueue = rabbitSection["CommentIngestionQueue"] ?? "livecommerce.comment.ingestion";
        });
        services.AddSingleton<IRabbitMQPublisher, RabbitMQPublisher>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICommentIngestionService, CommentIngestionService>();
        services.AddScoped<ICommentQueryService, CommentQueryService>();
        services.AddScoped<ICommentAssignmentService, CommentAssignmentService>();
        services.AddScoped<ILiveSessionService, LiveSessionService>();
        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IBlacklistService, BlacklistService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IChannelConnectionService, ChannelConnectionService>();
        services.AddScoped<IFollowUpService, FollowUpService>();

        return services;
    }
}
