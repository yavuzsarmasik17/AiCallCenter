using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class CallQueueOperator : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid CallQueueId { get; private set; }

    public Guid CallOperatorId { get; private set; }

    public int Priority { get; private set; }

    public bool IsActive { get; private set; }

    public DateTime AssignedAt { get; private set; }

    public Company Company { get; private set; } = null!;

    public CallQueue CallQueue { get; private set; } = null!;

    public CallOperator CallOperator { get; private set; } = null!;

    private CallQueueOperator()
    {
    }

    public CallQueueOperator(
        Guid companyId,
        Guid callQueueId,
        Guid callOperatorId,
        int priority = 1)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        if (callQueueId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kuyruk kimliği geçersiz.",
                nameof(callQueueId));
        }

        if (callOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Operatör kimliği geçersiz.",
                nameof(callOperatorId));
        }

        if (priority < 1 || priority > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                "Öncelik 1 ile 100 arasında olmalıdır.");
        }

        CompanyId = companyId;
        CallQueueId = callQueueId;
        CallOperatorId = callOperatorId;
        Priority = priority;
        IsActive = true;
        AssignedAt = DateTime.UtcNow;
    }

    public void ChangePriority(int priority)
    {
        if (priority < 1 || priority > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                "Öncelik 1 ile 100 arasında olmalıdır.");
        }

        Priority = priority;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}