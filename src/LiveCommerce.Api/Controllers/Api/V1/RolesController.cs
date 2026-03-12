using LiveCommerce.Application.Roles;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/roles")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<RoleListDto>>>> List(CancellationToken ct)
    {
        var list = await _service.GetByShopAsync(ct);
        return Ok(ApiResponse<List<RoleListDto>>.Ok(list));
    }
}
