namespace LiveCommerce.Application.Dashboard;

public interface IDashboardService
{
    Task<List<LiveSummaryDto>> GetLiveSummaryAsync(long shopId, long? liveSessionId, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<List<UserPerformanceDto>> GetUserPerformanceAsync(long shopId, long? liveSessionId, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
    Task<List<TopProductDto>> GetTopProductsAsync(long shopId, long? liveSessionId, int top, DateTime? fromUtc, DateTime? toUtc, CancellationToken ct = default);
}
