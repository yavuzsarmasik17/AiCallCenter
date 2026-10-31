using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.CallOperators.Dtos;

public sealed record UpdateCallOperatorStatusRequest(
    CallOperatorStatus Status);