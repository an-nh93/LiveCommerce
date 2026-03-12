using LiveCommerce.Application.Orders;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LiveCommerce.Api.Controllers.Api.V1;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _service;

    public OrdersController(IOrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CreateOrderResult>>> Create([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var result = await _service.CreateAsync(shopId, userId, request, ct);
        return Ok(ApiResponse<CreateOrderResult>.Ok(result));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderListDto>>>> List(
        [FromQuery] long? liveSessionId,
        [FromQuery] int? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (!GetShopAndUser(out var shopId, out _)) return Unauthorized();
        var filter = new OrderFilterDto
        {
            LiveSessionId = liveSessionId,
            Status = status.HasValue ? (OrderStatus)status : null,
            Search = search,
            Page = page,
            PageSize = Math.Clamp(pageSize, 1, 100)
        };
        var result = await _service.GetListAsync(shopId, filter, ct);
        return Ok(ApiResponse<PagedResult<OrderListDto>>.Ok(result));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> GetById(long id, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out _)) return Unauthorized();
        var order = await _service.GetByIdAsync(id, shopId, ct);
        if (order == null) return NotFound();
        return Ok(ApiResponse<OrderDetailDto>.Ok(order));
    }

    [HttpPost("{id:long}/status")]
    public async Task<ActionResult<ApiResponse<OrderDetailDto>>> UpdateStatus(long id, [FromBody] UpdateOrderStatusRequest body, CancellationToken ct)
    {
        if (!GetShopAndUser(out var shopId, out var userId)) return Unauthorized();
        var order = await _service.UpdateStatusAsync(id, (OrderStatus)body.Status, userId, shopId, body.Note, ct);
        if (order == null) return BadRequest(ApiResponse<OrderDetailDto>.Fail("Order not found or invalid."));
        return Ok(ApiResponse<OrderDetailDto>.Ok(order));
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
}

public class UpdateOrderStatusRequest
{
    public int Status { get; set; }
    public string? Note { get; set; }
}
