using LiveCommerce.Application.FollowUps;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/follow-ups")]
[Authorize]
public class FollowUpsController : ControllerBase
{
    private readonly IFollowUpService _service;

    public FollowUpsController(IFollowUpService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FollowUpListDto>>>> List([FromQuery] bool pendingOnly = false, CancellationToken ct = default)
    {
        if (!GetShopAndUser(out var shopId, out _)) return Unauthorized();
        var list = await _service.GetByShopAsync(shopId, pendingOnly, ct);
        return Ok(ApiResponse<List<FollowUpListDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FollowUpListDto>>> Create([FromBody] CreateFollowUpRequest request, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var f = await _service.CreateAsync(shopId, userId, request, ct);
        if (f == null) return BadRequest(ApiResponse<FollowUpListDto>.Fail("Invalid request."));
        return Ok(ApiResponse<FollowUpListDto>.Ok(f));
    }

    [HttpPost("{id:long}/done")]
    public async Task<ActionResult<ApiResponse<FollowUpListDto>>> MarkDone(long id, [FromBody] MarkDoneRequest? body, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var f = await _service.MarkDoneAsync(id, shopId, userId, body?.Note, ct);
        if (f == null) return NotFound(ApiResponse<FollowUpListDto>.Fail("Not found."));
        return Ok(ApiResponse<FollowUpListDto>.Ok(f));
    }

    private bool GetShopAndUser(out long shopId, out long userId)
    {
        var shopClaim = User.FindFirst("shop_id")?.Value;
        var userClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(shopClaim) || string.IsNullOrEmpty(userClaim) ||
            !long.TryParse(shopClaim, out shopId) || !long.TryParse(userClaim, out userId))
        {
            shopId = userId = 0;
            return false;
        }
        return true;
    }
}

public class MarkDoneRequest
{
    public string? Note { get; set; }
}
