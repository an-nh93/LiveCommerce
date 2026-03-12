namespace LiveCommerce.Application.Comments;

public interface ICommentIngestionService
{
    Task<bool> PublishAsync(CommentIngestionRequest request, CancellationToken ct = default);
}
