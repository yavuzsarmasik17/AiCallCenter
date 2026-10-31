using MediatR;

namespace AiCallCenter.Application.Dashboard.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(
    Guid CompanyId)
    : IRequest<DashboardSummaryResponse>;