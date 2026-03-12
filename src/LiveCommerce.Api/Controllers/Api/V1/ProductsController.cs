using LiveCommerce.Application.Products;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductQueryService _query;

    public ProductsController(IProductQueryService query)
    {
        _query = query;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<ProductListDto>>>> List(CancellationToken ct)
    {
        var shopIdClaim = User.FindFirst("shop_id")?.Value;
        if (string.IsNullOrEmpty(shopIdClaim) || !long.TryParse(shopIdClaim, out var shopId))
            return Unauthorized();
        var list = await _query.GetActiveByShopAsync(shopId, ct);
        return Ok(ApiResponse<List<ProductListDto>>.Ok(list));
    }
}
