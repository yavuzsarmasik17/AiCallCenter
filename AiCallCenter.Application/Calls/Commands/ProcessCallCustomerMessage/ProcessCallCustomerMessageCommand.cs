using MediatR;

namespace AiCallCenter.Application.Calls.Commands.ProcessCallCustomerMessage;

public sealed record ProcessCallCustomerMessageCommand(
    Guid CompanyId,
    Guid CallId,
    string Message)
    : IRequest<ProcessCallCustomerMessageResponse>;