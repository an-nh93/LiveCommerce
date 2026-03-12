using LiveCommerce.Application.Blacklists;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/blacklists")]
[Authorize]
public class BlacklistsController : ControllerBase
{
    private readonly IBlacklistService _service;

    public BlacklistsController(IBlacklistService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<BlacklistDto>>>> List(CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out _)) return Unauthorized();
        var list = await _service.GetByShopAsync(shopId, ct);
        return Ok(ApiResponse<List<BlacklistDto>>.Ok(list));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BlacklistDto>>> Create([FromBody] CreateBlacklistRequest request, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var b = await _service.CreateAsync(shopId, userId, request, ct);
        if (b == null) return BadRequest(ApiResponse<BlacklistDto>.Fail("Phone, Address hoặc Name bắt buộc."));
        return Ok(ApiResponse<BlacklistDto>.Ok(b));
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
