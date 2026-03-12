namespace LiveCommerce.Application.Comments;

public class CommentIngestionRequest
{
    public long ShopId { get; set; }
    public long ChannelConnectionId { get; set; }
    public long LiveSessionId { get; set; }
    public string ExternalCommentId { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime CommentTimeUtc { get; set; }
    public string? SenderExternalId { get; set; }
    public string? SenderName { get; set; }
    public string? RawPayloadJson { get; set; }
}
