namespace LiveCommerce.Application.FollowUps;

public interface IFollowUpService
{
    Task<List<FollowUpListDto>> GetByShopAsync(long shopId, bool pendingOnly, CancellationToken ct = default);
    Task<FollowUpListDto?> CreateAsync(long shopId, long userId, CreateFollowUpRequest request, CancellationToken ct = default);
    Task<FollowUpListDto?> MarkDoneAsync(long id, long shopId, long userId, string? note, CancellationToken ct = default);
}

public class CreateFollowUpRequest
{
    public long? CommentId { get; set; }
    public long? CustomerId { get; set; }
    public long AssignedUserId { get; set; }
    public DateTime TargetTimeUtc { get; set; }
    public string? Note { get; set; }
}
