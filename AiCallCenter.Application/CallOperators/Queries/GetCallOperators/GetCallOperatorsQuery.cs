using AiCallCenter.Application.CallOperators.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Queries.GetCallOperators;

public sealed record GetCallOperatorsQuery(
    Guid CompanyId)
    : IRequest<IReadOnlyList<CallOperatorDto>>;