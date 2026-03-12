using LiveCommerce.Application.Users;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserListDto>>>> List(CancellationToken ct)
    {
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(shopIdClaim) || !long.TryParse(shopIdClaim, out var shopId))
            return Unauthorized();
        var list = await _service.GetByShopAsync(shopId, ct);
        return Ok(ApiResponse<List<UserListDto>>.Ok(list));
    }
}
