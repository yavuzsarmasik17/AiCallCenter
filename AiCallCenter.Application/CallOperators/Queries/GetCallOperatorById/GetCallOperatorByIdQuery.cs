using AiCallCenter.Application.CallOperators.Dtos;
using MediatR;

namespace AiCallCenter.Application.CallOperators.Queries
    .GetCallOperatorById;

public sealed record GetCallOperatorByIdQuery(
    Guid CompanyId,
    Guid OperatorId)
    : IRequest<CallOperatorDto>;