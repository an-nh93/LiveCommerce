namespace LiveCommerce.Application.Users;

public class UserListDto
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string RoleCode { get; set; } = null!;
    public bool IsActive { get; set; }
}
