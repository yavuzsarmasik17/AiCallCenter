using AiCallCenter.Application.Calls.Dtos;
using MediatR;

namespace AiCallCenter.Application.Calls.Queries.GetCalls;

public sealed record GetCallsQuery(
    Guid CompanyId)
    : IRequest<IReadOnlyList<CallDto>>;
