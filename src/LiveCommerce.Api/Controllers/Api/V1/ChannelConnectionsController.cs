using LiveCommerce.Application.Channels;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/channel-connections")]
[Authorize]
public class ChannelConnectionsController : ControllerBase
{
    private readonly IChannelConnectionService _service;

    public ChannelConnectionsController(IChannelConnectionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ChannelConnectionDto>>>> List(CancellationToken ct)
    {
        if (!GetShopId(out var shopId)) return Unauthorized();
        var list = await _service.GetByShopAsync(shopId, ct);
        return Ok(ApiResponse<List<ChannelConnectionDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChannelConnectionDto>>> Create([FromBody] CreateChannelRequest request, CancellationToken ct)
    {
        if (!GetShopId(out var shopId)) return Unauthorized();
        var c = await _service.CreateAsync(shopId, request, ct);
        if (c == null) return BadRequest(ApiResponse<ChannelConnectionDto>.Fail("Invalid request."));
        return Ok(ApiResponse<ChannelConnectionDto>.Ok(c));
    }

    private bool GetShopId(out long shopId)
    {
        var claim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(claim) || !long.TryParse(claim, out shopId)) { shopId = 0; return false; }
        return true;
    }
}
