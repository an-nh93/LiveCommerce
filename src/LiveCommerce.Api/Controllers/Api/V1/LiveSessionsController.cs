using LiveCommerce.Application.LiveSessions;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/live-sessions")]
[Authorize]
public class LiveSessionsController : ControllerBase
{
    private readonly ILiveSessionService _service;

    public LiveSessionsController(ILiveSessionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LiveSessionDto>>>> List(CancellationToken ct)
    {
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(shopIdClaim) || !long.TryParse(shopIdClaim, out var shopId))
            return Unauthorized();
        var list = await _service.GetActiveByShopAsync(shopId, ct);
        return Ok(ApiResponse<List<LiveSessionDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LiveSessionDto>>> Create([FromBody] CreateLiveSessionRequest request, CancellationToken ct)
    {
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(shopIdClaim) || !long.TryParse(shopIdClaim, out var shopId))
            return Unauthorized();
        var session = await _service.CreateAsync(shopId, request, ct);
        if (session == null) return BadRequest(ApiResponse<LiveSessionDto>.Fail("Invalid channel or shop."));
        return Ok(ApiResponse<LiveSessionDto>.Ok(session));
    }
}
