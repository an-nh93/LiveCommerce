namespace LiveCommerce.Application.FollowUps;

public class FollowUpListDto
{
    public long Id { get; set; }
    public DateTime TargetTimeUtc { get; set; }
    public int Status { get; set; }
    public string? Note { get; set; }
    public string? AssignedUserName { get; set; }
    public long? CommentId { get; set; }
    public long? CustomerId { get; set; }
}
