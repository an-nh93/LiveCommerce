using LiveCommerce.Domain.Enums;

namespace LiveCommerce.Application.Channels;

public class ChannelConnectionDto
{
    public long Id { get; set; }
    public ChannelType ChannelType { get; set; }
    public string ChannelName { get; set; } = null!;
    public bool IsActive { get; set; }
}
