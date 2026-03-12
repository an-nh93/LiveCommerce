using System.Text;
using System.Text.Json;
using LiveCommerce.Application.Comments;
using LiveCommerce.Domain.Entities;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Infrastructure.Messaging;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LiveCommerce.Worker;

public class CommentConsumerWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CommentConsumerWorker> _logger;
    private readonly RabbitMQOptions _options;

    public CommentConsumerWorker(IServiceProvider services, IOptions<RabbitMQOptions> options, ILogger<CommentConsumerWorker> logger)
    {
        _services = services;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeLoopAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Comment consumer error");
            }
            await Task.Delay(5000, stoppingToken);
        }
    }

    private async Task ConsumeLoopAsync(CancellationToken ct)
    {
        var factory = new RabbitMQ.Client.ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password
        };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(_options.CommentIngestionQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var consumer = new RabbitMQ.Client.Events.EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var request = JsonSerializer.Deserialize<CommentIngestionRequest>(json);
                if (request == null) { channel.BasicNack(ea.DeliveryTag, false, false); return; }

                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var exists = await db.Comments.AnyAsync(c => c.LiveSessionId == request.LiveSessionId && c.ExternalCommentId == request.ExternalCommentId, ct);
                if (exists) { channel.BasicAck(ea.DeliveryTag, false); return; }

                var comment = new Comment
                {
                    ShopId = request.ShopId,
                    LiveSessionId = request.LiveSessionId,
                    ExternalCommentId = request.ExternalCommentId,
                    Content = request.Content,
                    CommentTimeUtc = request.CommentTimeUtc,
                    SenderExternalId = request.SenderExternalId,
                    SenderName = request.SenderName,
                    RawPayloadJson = request.RawPayloadJson,
                    Status = CommentStatus.New,
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Comments.Add(comment);
                await db.SaveChangesAsync(ct);
                channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Comment saved: Id={Id}, LiveSessionId={LiveSessionId}", comment.Id, comment.LiveSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Process comment message failed");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };
        channel.BasicConsume(_options.CommentIngestionQueue, autoAck: false, consumerTag: "", noLocal: false, exclusive: false, arguments: null, consumer);
        await Task.Delay(Timeout.Infinite, ct);
    }
}
