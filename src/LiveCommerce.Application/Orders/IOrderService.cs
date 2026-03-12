using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Orders;

public interface IOrderService
{
    Task<OrderDetailDto?> CreateAsync(long shopId, long userId, CreateOrderRequest request, CancellationToken ct = default);
    Task<LiveCommerce.Shared.PagedResult<OrderListDto>> GetListAsync(long shopId, OrderFilterDto filter, CancellationToken ct = default);
    Task<OrderDetailDto?> GetByIdAsync(long orderId, long shopId, CancellationToken ct = default);
    Task<OrderDetailDto?> UpdateStatusAsync(long orderId, OrderStatus status, long userId, long shopId, string? note, CancellationToken ct = default);
}

public class OrderFilterDto
{
    public long? LiveSessionId { get; set; }
    public OrderStatus? Status { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
