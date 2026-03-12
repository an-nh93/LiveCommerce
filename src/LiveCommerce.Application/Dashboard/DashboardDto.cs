namespace LiveCommerce.Application.Dashboard;

public class LiveSummaryDto
{
    public long LiveSessionId { get; set; }
    public string LiveSessionName { get; set; } = null!;
    public int CommentCount { get; set; }
    public int NewCount { get; set; }
    public int OrderedCount { get; set; }
    public int OrderCount { get; set; }
    public decimal EstimatedRevenue { get; set; }
}

public class UserPerformanceDto
{
    public long UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int CommentsHandled { get; set; }
    public int OrdersCreated { get; set; }
    public decimal OrderRevenue { get; set; }
}

public class TopProductDto
{
    public long ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}
