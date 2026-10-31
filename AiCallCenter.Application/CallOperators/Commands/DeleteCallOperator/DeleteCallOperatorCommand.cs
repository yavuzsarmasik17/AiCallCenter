using MediatR;

namespace AiCallCenter.Application.CallOperators.Commands
    .DeleteCallOperator;

public sealed record DeleteCallOperatorCommand(
    Guid CompanyId,
    Guid OperatorId)
    : IRequest;