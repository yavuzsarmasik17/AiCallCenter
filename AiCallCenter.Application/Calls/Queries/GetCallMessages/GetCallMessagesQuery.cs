using AiCallCenter.Application.Calls.Dtos;
using MediatR;

namespace AiCallCenter.Application.Calls.Queries.GetCallMessages;

public sealed record GetCallMessagesQuery(
    Guid CompanyId,
    Guid CallId)
    : IRequest<IReadOnlyList<CallMessageDto>>;