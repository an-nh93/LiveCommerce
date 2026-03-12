using LiveCommerce.Application.Comments;
using LiveCommerce.Infrastructure.Persistence;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentIngestionService _ingestion;
    private readonly ICommentQueryService _query;
    private readonly AppDbContext _db;

    public CommentsController(ICommentIngestionService ingestion, ICommentQueryService query, AppDbContext db)
    {
        _ingestion = ingestion;
        _query = query;
        _db = db;
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> Webhook([FromBody] CommentWebhookPayload payload, CancellationToken ct)
    {
        if (payload.LiveSessionId == 0 || string.IsNullOrWhiteSpace(payload.ExternalCommentId) || string.IsNullOrWhiteSpace(payload.Content))
            return BadRequest(ApiResponse<object>.Fail("LiveSessionId, ExternalCommentId and Content are required."));

        var session = await _db.LiveSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == payload.LiveSessionId && s.IsActive && s.EndedAtUtc == null, ct);
        if (session == null)
            return NotFound(ApiResponse<object>.Fail("Live session not found or not active."));

        var request = new CommentIngestionRequest
        {
            ShopId = session.ShopId,
            ChannelConnectionId = session.ChannelConnectionId,
            LiveSessionId = session.Id,
            ExternalCommentId = payload.ExternalCommentId,
            Content = payload.Content,
            CommentTimeUtc = payload.CommentTimeUtc ?? DateTime.UtcNow,
            SenderExternalId = payload.SenderExternalId,
            SenderName = payload.SenderName,
            RawPayloadJson = payload.RawPayloadJson
        };
        var ok = await _ingestion.PublishAsync(request, ct);
        if (!ok) return StatusCode(500, ApiResponse<object>.Fail("Failed to enqueue comment."));
        return Ok(ApiResponse<object>.Ok(new { accepted = true }));
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<CommentListDto>>>> List(
        [FromQuery] long? liveSessionId,
        [FromQuery] int? status,
        [FromQuery] long? assignedUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(shopIdClaim) || !long.TryParse(shopIdClaim, out var shopId))
            return Unauthorized();

        var filter = new CommentFilterDto
        {
            LiveSessionId = liveSessionId,
            Status = status.HasValue ? (Domain.Enums.CommentStatus)status : null,
            AssignedUserId = assignedUserId,
            Page = page,
            PageSize = Math.Clamp(pageSize, 1, 100)
        };
        var result = await _query.GetListAsync(shopId, filter, ct);
        return Ok(ApiResponse<PagedResult<CommentListDto>>.Ok(result));
    }
}

public class CommentWebhookPayload
{
    public long LiveSessionId { get; set; }
    public string ExternalCommentId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime? CommentTimeUtc { get; set; }
    public string? SenderExternalId { get; set; }
    public string? SenderName { get; set; }
    public string? RawPayloadJson { get; set; }
}
