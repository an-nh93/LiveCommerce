using LiveCommerce.Application.Orders;
using LiveCommerce.Application.Blacklists;
using LiveCommerce.Domain.Entities;
using LiveCommerce.Domain.Enums;
using LiveCommerce.Infrastructure.Persistence;
using LiveCommerce.Shared;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _db;
    private readonly IBlacklistService _blacklist;

    public OrderService(AppDbContext db, IBlacklistService blacklist)
    {
        _db = db;
        _blacklist = blacklist;
    }

    public async Task<CreateOrderResult> CreateAsync(long shopId, long userId, CreateOrderRequest request, CancellationToken ct = default)
    {
        var riskWarning = await _blacklist.IsBlacklistedAsync(shopId, request.ReceiverPhone, request.ReceiverAddress, ct) ? "Số điện thoại hoặc địa chỉ nằm trong blacklist." : null;
        var orderNo = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8]}";
        var order = new Order
        {
            ShopId = shopId,
            AssignedUserId = userId,
            OrderNo = orderNo,
            LiveSessionId = request.LiveSessionId,
            CommentId = request.CommentId,
            CustomerId = request.CustomerId,
            ReceiverName = request.ReceiverName,
            ReceiverPhone = request.ReceiverPhone,
            ReceiverAddress = request.ReceiverAddress,
            Note = request.Note,
            ShippingFee = request.ShippingFee,
            Discount = request.Discount,
            Status = OrderStatus.PendingConfirm,
            CreatedAtUtc = DateTime.UtcNow
        };
        decimal subTotal = 0;
        foreach (var item in request.Items.Where(i => i.Quantity > 0))
        {
            var lineTotal = item.UnitPrice * item.Quantity;
            subTotal += lineTotal;
            order.Items.Add(new OrderItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = lineTotal,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        order.SubTotal = subTotal;
        order.TotalAmount = subTotal + order.ShippingFee - order.Discount;

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);

            if (request.CommentId.HasValue)
            {
                var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == request.CommentId && c.ShopId == shopId, ct);
                if (comment != null)
                {
                    comment.Status = CommentStatus.Ordered;
                    comment.UpdatedAtUtc = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);
                }
            }

            if (request.CustomerId.HasValue)
            {
                var customer = await _db.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.ShopId == shopId, ct);
                if (customer != null)
                {
                    customer.TotalOrders += 1;
                    customer.TotalSpent += order.TotalAmount;
                    customer.LastOrderDateUtc = DateTime.UtcNow;
                    customer.UpdatedAtUtc = DateTime.UtcNow;
                    await _db.SaveChangesAsync(ct);
                }
            }

            await tx.CommitAsync(ct);
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }

        var orderDetail = await GetByIdAsync(order.Id, shopId, ct);
        return new CreateOrderResult { Order = orderDetail!, RiskWarning = riskWarning };
    }

    public async Task<PagedResult<OrderListDto>> GetListAsync(long shopId, OrderFilterDto filter, CancellationToken ct = default)
    {
        var q = _db.Orders.AsNoTracking().Where(o => o.ShopId == shopId)
            .Include(o => o.AssignedUser)
            .AsQueryable();
        if (filter.LiveSessionId.HasValue) q = q.Where(o => o.LiveSessionId == filter.LiveSessionId);
        if (filter.Status.HasValue) q = q.Where(o => o.Status == filter.Status.Value);
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            q = q.Where(o => o.OrderNo.Contains(s) || (o.ReceiverPhone != null && o.ReceiverPhone.Contains(s)) || (o.ReceiverName != null && o.ReceiverName.Contains(s)));
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(o => o.CreatedAtUtc)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(o => new OrderListDto
            {
                Id = o.Id,
                OrderNo = o.OrderNo,
                LiveSessionId = o.LiveSessionId,
                ReceiverName = o.ReceiverName,
                ReceiverPhone = o.ReceiverPhone,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                AssignedUserName = o.AssignedUser != null ? o.AssignedUser.DisplayName ?? o.AssignedUser.Username : null,
                CreatedAtUtc = o.CreatedAtUtc
            })
            .ToListAsync(ct);
        return new PagedResult<OrderListDto> { Items = items, TotalCount = total, Page = filter.Page, PageSize = filter.PageSize };
    }

    public async Task<OrderDetailDto?> GetByIdAsync(long orderId, long shopId, CancellationToken ct = default)
    {
        var o = await _db.Orders.AsNoTracking()
            .Include(x => x.AssignedUser)
            .Include(x => x.Items).ThenInclude(i => i.ProductVariant).ThenInclude(v => v!.Product)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.ShopId == shopId, ct);
        if (o == null) return null;
        return new OrderDetailDto
        {
            Id = o.Id,
            OrderNo = o.OrderNo,
            LiveSessionId = o.LiveSessionId,
            CommentId = o.CommentId,
            CustomerId = o.CustomerId,
            ReceiverName = o.ReceiverName,
            ReceiverPhone = o.ReceiverPhone,
            ReceiverAddress = o.ReceiverAddress,
            Note = o.Note,
            SubTotal = o.SubTotal,
            ShippingFee = o.ShippingFee,
            Discount = o.Discount,
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            AssignedUserName = o.AssignedUser != null ? o.AssignedUser.DisplayName ?? o.AssignedUser.Username : null,
            CreatedAtUtc = o.CreatedAtUtc,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductVariantId = i.ProductVariantId,
                ProductName = i.ProductVariant.Product.Name,
                Sku = i.ProductVariant.Sku,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal
            }).ToList()
        };
    }

    public async Task<OrderDetailDto?> UpdateStatusAsync(long orderId, OrderStatus status, long userId, long shopId, string? note, CancellationToken ct = default)
    {
        var order = await _db.Orders.Include(o => o.AssignedUser).FirstOrDefaultAsync(o => o.Id == orderId && o.ShopId == shopId, ct);
        if (order == null) return null;
        var fromStatus = order.Status;
        order.Status = status;
        order.UpdatedAtUtc = DateTime.UtcNow;
        _db.OrderLogs.Add(new OrderLog
        {
            OrderId = order.Id,
            ActorUserId = userId,
            FromStatus = fromStatus,
            ToStatus = status,
            Note = note,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
        return await GetByIdAsync(orderId, shopId, ct);
    }
}
