using System.Text.Json;
using LiveCommerce.Application.Comments;
using LiveCommerce.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;

namespace LiveCommerce.Infrastructure.Services;

public class CommentIngestionService : ICommentIngestionService
{
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<CommentIngestionService> _logger;

    public CommentIngestionService(IRabbitMQPublisher publisher, ILogger<CommentIngestionService> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task<bool> PublishAsync(CommentIngestionRequest request, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(request);
            _publisher.PublishCommentIngestion(json);
            _logger.LogInformation("Comment ingestion published: ShopId={ShopId}, LiveSessionId={LiveSessionId}, ExternalId={ExternalId}",
                request.ShopId, request.LiveSessionId, request.ExternalCommentId);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish comment ingestion");
            return Task.FromResult(false);
        }
    }
}
