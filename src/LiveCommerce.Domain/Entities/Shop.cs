namespace LiveCommerce.Domain.Entities;

public class Shop : Common.BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ChannelConnection> ChannelConnections { get; set; } = new List<ChannelConnection>();
    public ICollection<LiveSession> LiveSessions { get; set; } = new List<LiveSession>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    public ICollection<Blacklist> Blacklists { get; set; } = new List<Blacklist>();
}
