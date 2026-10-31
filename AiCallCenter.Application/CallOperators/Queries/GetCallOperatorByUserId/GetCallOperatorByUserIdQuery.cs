using AiCallCenter.Application.CallOperators.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Queries
    .GetCallOperatorByUserId;

public sealed record GetCallOperatorByUserIdQuery(
    Guid CompanyId,
    Guid UserId)
    : IRequest<CallOperatorDto>;
