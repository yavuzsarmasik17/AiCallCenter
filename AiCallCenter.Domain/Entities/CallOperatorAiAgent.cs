using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class CallOperatorAiAgent : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid CallOperatorId { get; private set; }

    public Guid AiAgentId { get; private set; }

    public bool IsPrimary { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime AssignedAt { get; private set; }

    public Company Company { get; private set; } = null!;

    public CallOperator CallOperator { get; private set; } = null!;

    public AiAgent AiAgent { get; private set; } = null!;

    private CallOperatorAiAgent()
    {
    }

    public CallOperatorAiAgent(
        Guid companyId,
        Guid callOperatorId,
        Guid aiAgentId,
        bool isPrimary = false)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        if (callOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Operatör kimliği geçersiz.",
                nameof(callOperatorId));
        }

        if (aiAgentId == Guid.Empty)
        {
            throw new ArgumentException(
                "AI ajanı kimliği geçersiz.",
                nameof(aiAgentId));
        }

        CompanyId = companyId;
        CallOperatorId = callOperatorId;
        AiAgentId = aiAgentId;
        IsPrimary = isPrimary;
        IsActive = true;
        AssignedAt = DateTime.UtcNow;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
        IsActive = true;
    }

    public void RemovePrimary()
    {
        IsPrimary = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
        IsPrimary = false;
    }
}