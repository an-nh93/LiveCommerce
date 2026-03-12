namespace LiveCommerce.Application.Roles;

public class RoleListDto
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public List<string> PermissionCodes { get; set; } = new();
}
