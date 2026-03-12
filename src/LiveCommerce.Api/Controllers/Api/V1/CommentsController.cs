using LiveCommerce.Application.Comments;
using LiveCommerce.Api.Hubs;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Infrastructure.Persistence;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/comments")]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly ICommentIngestionService _ingestion;
    private readonly ICommentQueryService _query;
    private readonly ICommentAssignmentService _assignment;
    private readonly AppDbContext _db;
    private readonly IHubContext<CommentHub> _hub;

    public CommentsController(ICommentIngestionService ingestion, ICommentQueryService query, ICommentAssignmentService assignment, AppDbContext db, IHubContext<CommentHub> hub)
    {
        _ingestion = ingestion;
        _query = query;
        _assignment = assignment;
        _db = db;
        _hub = hub;
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

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<CommentDetailDto>>> GetById(long id, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out _)) return Unauthorized();
        var dto = await _assignment.GetByIdAsync(id, shopId, ct);
        if (dto == null) return NotFound();
        return Ok(ApiResponse<CommentDetailDto>.Ok(dto));
    }

    [HttpPost("{id:long}/take")]
    public async Task<ActionResult<ApiResponse<CommentListDto>>> Take(long id, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var dto = await _assignment.TakeAsync(id, userId, shopId, ct);
        if (dto == null) return BadRequest(ApiResponse<CommentListDto>.Fail("Cannot take this comment."));
        await PushCommentUpdated(dto.LiveSessionId, dto);
        return Ok(ApiResponse<CommentListDto>.Ok(dto));
    }

    [HttpPost("{id:long}/assign")]
    public async Task<ActionResult<ApiResponse<CommentListDto>>> Assign(long id, [FromBody] AssignRequest body, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var dto = await _assignment.AssignAsync(id, body.AssignToUserId, userId, shopId, ct);
        if (dto == null) return BadRequest(ApiResponse<CommentListDto>.Fail("Cannot assign."));
        await PushCommentUpdated(dto.LiveSessionId, dto);
        return Ok(ApiResponse<CommentListDto>.Ok(dto));
    }

    [HttpPost("{id:long}/status")]
    public async Task<ActionResult<ApiResponse<CommentListDto>>> UpdateStatus(long id, [FromBody] UpdateStatusRequest body, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var dto = await _assignment.UpdateStatusAsync(id, (CommentStatus)body.Status, userId, shopId, body.Note, ct);
        if (dto == null) return BadRequest(ApiResponse<CommentListDto>.Fail("Invalid status transition."));
        await PushCommentUpdated(dto.LiveSessionId, dto);
        return Ok(ApiResponse<CommentListDto>.Ok(dto));
    }

    private bool GetShopAndUser(out long shopId, out long userId)
    {
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(shopIdClaim) || string.IsNullOrEmpty(userIdClaim) ||
            !long.TryParse(shopIdClaim, out shopId) || !long.TryParse(userIdClaim, out userId))
        {
            shopId = userId = 0;
            return false;
        }
        return true;
    }

    private async Task PushCommentUpdated(long liveSessionId, CommentListDto dto)
    {
        await _hub.Clients.Group($"live_{liveSessionId}").SendAsync("CommentUpdated", dto);
    }
}

public class AssignRequest { public long AssignToUserId { get; set; } }
public class UpdateStatusRequest { public int Status { get; set; } public string? Note { get; set; } }

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
