using System.Text;
using LiveCommerce.Infrastructure;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
    cfg.Enrich.FromLogContext();
    cfg.WriteTo.Console();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "LiveCommerce API", Version = "v1" });
});

var jwtKey = builder.Configuration["Jwt:Key"] ?? "LiveCommerce-SuperSecret-Key-Min-32-Chars!!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "LiveCommerce",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "LiveCommerce",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                var accessToken = ctx.Request.Query["access_token"];
                var path = ctx.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    ctx.Token = accessToken;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? new[] { "http://localhost:5173", "http://localhost:3000" })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
var rabbitHost = builder.Configuration["RabbitMQ:HostName"] ?? "localhost";
var rabbitUri = new Uri($"amqp://{builder.Configuration["RabbitMQ:UserName"] ?? "guest"}:{builder.Configuration["RabbitMQ:Password"] ?? "guest"}@{rabbitHost}:{builder.Configuration["RabbitMQ:Port"] ?? "5672"}");
builder.Services.AddSingleton<IConnection>(_ =>
{
    var factory = new ConnectionFactory { Uri = rabbitUri };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
});
builder.Services.AddHealthChecks()
    .AddNpgSql(connStr ?? "Host=localhost;Database=livecommerce;Username=postgres;Password=postgres", name: "postgres")
    .AddRabbitMQ(name: "rabbitmq");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<LiveCommerce.Api.Hubs.CommentHub>("/hubs/comments");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new { name = e.Key, status = e.Value.Status.ToString(), description = e.Value.Description?.ToString() })
        });
        await ctx.Response.WriteAsync(result);
    }
});

// Apply migrations on startup in dev (optional)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await db.Database.MigrateAsync();
        await SeedData.SeedAsync(db, app.Logger, default);
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Migration or DB not available yet. Run migrations manually.");
    }
}

app.Run();
