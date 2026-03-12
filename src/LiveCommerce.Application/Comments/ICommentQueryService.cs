using LiveCommerce.Shared;

namespace LiveCommerce.Application.Comments;

public interface ICommentQueryService
{
    Task<PagedResult<CommentListDto>> GetListAsync(long shopId, CommentFilterDto filter, CancellationToken ct = default);
}
