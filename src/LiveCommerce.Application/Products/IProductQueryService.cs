namespace LiveCommerce.Application.Products;

public interface IProductQueryService
{
    Task<List<ProductListDto>> GetActiveByShopAsync(long shopId, CancellationToken ct = default);
}
