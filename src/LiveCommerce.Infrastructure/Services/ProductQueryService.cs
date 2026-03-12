using LiveCommerce.Application.Products;
using LiveCommerce.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LiveCommerce.Infrastructure.Services;

public class ProductQueryService : IProductQueryService
{
    private readonly AppDbContext _db;

    public ProductQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProductListDto>> GetActiveByShopAsync(long shopId, CancellationToken ct = default)
    {
        return await _db.Products
            .AsNoTracking()
            .Where(p => p.ShopId == shopId && p.IsActive)
            .Include(p => p.Variants.Where(v => v.IsActive))
            .OrderBy(p => p.Code)
            .Select(p => new ProductListDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                BasePrice = p.BasePrice,
                IsActive = p.IsActive,
                Variants = p.Variants.Where(v => v.IsActive).Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    Sku = v.Sku,
                    Color = v.Color,
                    Size = v.Size,
                    Price = v.Price
                }).ToList()
            })
            .ToListAsync(ct);
    }
}
