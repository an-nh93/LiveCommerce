using LiveCommerce.Application.Dashboard;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet("live-summary")]
    public async Task<ActionResult<ApiResponse<List<LiveSummaryDto>>>> LiveSummary(
        [FromQuery] long? liveSessionId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct = default)
    {
        if (!GetShopId(out var shopId)) return Unauthorized();
        var list = await _service.GetLiveSummaryAsync(shopId, liveSessionId, fromUtc, toUtc, ct);
        return Ok(ApiResponse<List<LiveSummaryDto>>.Ok(list));
    }

    [HttpGet("user-performance")]
    public async Task<ActionResult<ApiResponse<List<UserPerformanceDto>>>> UserPerformance(
        [FromQuery] long? liveSessionId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken ct = default)
    {
        if (!GetShopId(out var shopId)) return Unauthorized();
        var list = await _service.GetUserPerformanceAsync(shopId, liveSessionId, fromUtc, toUtc, ct);
        return Ok(ApiResponse<List<UserPerformanceDto>>.Ok(list));
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<ApiResponse<List<TopProductDto>>>> TopProducts(
        [FromQuery] long? liveSessionId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int top = 10,
        CancellationToken ct = default)
    {
        if (!GetShopId(out var shopId)) return Unauthorized();
        var list = await _service.GetTopProductsAsync(shopId, liveSessionId, Math.Clamp(top, 1, 50), fromUtc, toUtc, ct);
        return Ok(ApiResponse<List<TopProductDto>>.Ok(list));
    }

    private bool GetShopId(out long shopId)
    {
        var claim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(claim) || !long.TryParse(claim, out shopId)) { shopId = 0; return false; }
        return true;
    }
}
