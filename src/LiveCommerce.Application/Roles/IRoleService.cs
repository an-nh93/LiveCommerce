namespace LiveCommerce.Application.Roles;

public interface IRoleService
{
    Task<List<RoleListDto>> GetByShopAsync(CancellationToken ct = default);
}
