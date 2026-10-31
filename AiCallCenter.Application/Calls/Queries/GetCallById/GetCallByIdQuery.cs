using AiCallCenter.Application.Calls.Dtos;
using MediatR;

namespace AiCallCenter.Application.Calls.Queries.GetCallById;

public sealed record GetCallByIdQuery(
    Guid CompanyId,
    Guid CallId)
    : IRequest<CallDto>;
