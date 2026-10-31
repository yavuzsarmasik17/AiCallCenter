using AiCallCenter.Application.Common.Dashboard;
using MediatR;

namespace AiCallCenter.Application.Dashboard.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler
    : IRequestHandler<
        GetDashboardSummaryQuery,
        DashboardSummaryResponse>
{
    private readonly IDashboardReadService
        _dashboardReadService;

    public GetDashboardSummaryQueryHandler(
        IDashboardReadService dashboardReadService)
    {
        _dashboardReadService = dashboardReadService;
    }

    public Task<DashboardSummaryResponse> Handle(
        GetDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(request.CompanyId));
        }

        return _dashboardReadService.GetSummaryAsync(
            request.CompanyId,
            cancellationToken);
    }
}