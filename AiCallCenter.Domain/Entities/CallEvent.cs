using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class CallEvent : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid CallId { get; private set; }

    public Guid? OperatorId { get; private set; }

    public Guid? AiAgentId { get; private set; }

    public CallEventType EventType { get; private set; }

    public string? Description { get; private set; }

    public string? MetadataJson { get; private set; }

    public DateTime OccurredAt { get; private set; }

    public Company Company { get; private set; } = null!;

    public Call Call { get; private set; } = null!;

    public CallOperator? Operator { get; private set; }

    public AiAgent? AiAgent { get; private set; }

    private CallEvent()
    {
    }

    public CallEvent(
        Guid companyId,
        Guid callId,
        CallEventType eventType,
        string? description = null,
        Guid? operatorId = null,
        Guid? aiAgentId = null,
        string? metadataJson = null)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        if (callId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.",
                nameof(callId));
        }

        CompanyId = companyId;
        CallId = callId;
        EventType = eventType;
        Description = NormalizeOptionalValue(description);
        OperatorId = NormalizeGuid(operatorId);
        AiAgentId = NormalizeGuid(aiAgentId);
        MetadataJson = NormalizeOptionalValue(metadataJson);
        OccurredAt = DateTime.UtcNow;
    }

    public void UpdateDescription(string? description)
    {
        Description = NormalizeOptionalValue(description);
    }

    public void SetMetadata(string? metadataJson)
    {
        MetadataJson = NormalizeOptionalValue(metadataJson);
    }

    private static Guid? NormalizeGuid(Guid? value)
    {
        return value.HasValue && value.Value != Guid.Empty
            ? value
            : null;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}