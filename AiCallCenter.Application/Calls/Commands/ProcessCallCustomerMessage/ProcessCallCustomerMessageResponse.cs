using AiCallCenter.Application.Calls.Dtos;

namespace AiCallCenter.Application.Calls.Commands.ProcessCallCustomerMessage;

public sealed class ProcessCallCustomerMessageResponse
{
    public Guid CallId { get; init; }

    public CallMessageDto CustomerMessage { get; init; } = null!;

    public CallMessageDto? AiMessage { get; init; }

    public string? Answer { get; init; }

    public bool HandledByHuman { get; init; }
}