namespace LiveCommerce.Application.Users;

public interface IUserService
{
    Task<List<UserListDto>> GetByShopAsync(long shopId, CancellationToken ct = default);
}
