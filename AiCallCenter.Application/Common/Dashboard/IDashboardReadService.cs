using AiCallCenter.Application.Dashboard.Queries.GetDashboardSummary;

namespace AiCallCenter.Application.Common.Dashboard;

public interface IDashboardReadService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);
}