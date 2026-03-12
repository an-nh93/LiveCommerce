namespace LiveCommerce.Application.LiveSessions;

public class LiveSessionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ExternalLiveId { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public bool IsActive { get; set; }
}
