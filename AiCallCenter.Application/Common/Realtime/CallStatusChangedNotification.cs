using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Common.Realtime;

public sealed record CallStatusChangedNotification(
    Guid CompanyId,
    Guid CallId,
    CallStatus Status,
    DateTime ChangedAt,
    string? Description);