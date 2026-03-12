namespace LiveCommerce.Domain.Entities;

public class User : Common.BaseEntity
{
    public long ShopId { get; set; }
    public Shop Shop { get; set; } = null!;
    public long RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Comment> AssignedComments { get; set; } = new List<Comment>();
    public ICollection<Order> AssignedOrders { get; set; } = new List<Order>();
    public ICollection<FollowUp> AssignedFollowUps { get; set; } = new List<FollowUp>();
}
