using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class CallQueueItem : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid CallQueueId { get; private set; }

    public Guid CallId { get; private set; }

    public Guid? AssignedOperatorId { get; private set; }

    public CallQueueStatus Status { get; private set; }

    public int Priority { get; private set; }

    public string QueueName { get; private set; } = string.Empty;

    public string? TransferReason { get; private set; }

    // AI tarafından operatöre bırakılan çağrı özeti.
    public string? AiHandoffSummary { get; private set; }

    public DateTime WaitingSince { get; private set; }

    public DateTime? AssignedAt { get; private set; }

    public DateTime? AcceptedAt { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public DateTime? CancelledAt { get; private set; }

    public DateTime? ExpiresAt { get; private set; }

    public CallQueue CallQueue { get; private set; } = null!;

    public Call Call { get; private set; } = null!;

    public Company Company { get; private set; } = null!;

    public CallOperator? AssignedOperator { get; private set; }

    private CallQueueItem()
    {
    }

    public CallQueueItem(
        Guid companyId,
        Guid callQueueId,
        Guid callId,
        string queueName,
        int priority = 1,
        string? transferReason = null,
        DateTime? expiresAt = null,
        string? aiHandoffSummary = null)
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

        if (callId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.",
                nameof(callId));
        }

        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException(
                "Kuyruk adı boş olamaz.",
                nameof(queueName));
        }

        if (priority < 1 || priority > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                "Öncelik değeri 1 ile 100 arasında olmalıdır.");
        }

        CompanyId = companyId;
        CallQueueId = callQueueId;
        CallId = callId;
        QueueName = queueName.Trim();
        Priority = priority;

        TransferReason =
            NormalizeOptionalValue(
                transferReason);

        AiHandoffSummary =
            NormalizeOptionalValue(
                aiHandoffSummary);

        Status = CallQueueStatus.Waiting;
        WaitingSince = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    public void AssignToOperator(
        Guid operatorId)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "Operatör kimliği geçersiz.",
                nameof(operatorId));
        }

        if (Status != CallQueueStatus.Waiting)
        {
            throw new InvalidOperationException(
                "Yalnızca bekleyen çağrılar operatöre atanabilir.");
        }

        AssignedOperatorId = operatorId;
        AssignedAt = DateTime.UtcNow;
        Status = CallQueueStatus.Assigned;
    }

    public void Accept()
    {
        if (Status != CallQueueStatus.Assigned)
        {
            throw new InvalidOperationException(
                "Yalnızca atanmış çağrılar kabul edilebilir.");
        }

        AcceptedAt = DateTime.UtcNow;
        Status = CallQueueStatus.Accepted;
    }

    public void Complete()
    {
        if (Status == CallQueueStatus.Completed)
        {
            return;
        }

        CompletedAt = DateTime.UtcNow;
        Status = CallQueueStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == CallQueueStatus.Completed)
        {
            throw new InvalidOperationException(
                "Tamamlanmış kuyruk kaydı iptal edilemez.");
        }

        CancelledAt = DateTime.UtcNow;
        Status = CallQueueStatus.Cancelled;
    }

    public void Expire()
    {
        if (Status != CallQueueStatus.Waiting &&
            Status != CallQueueStatus.Assigned)
        {
            throw new InvalidOperationException(
                "Bu kuyruk kaydı süresi dolmuş olarak işaretlenemez.");
        }

        Status = CallQueueStatus.Expired;
        CancelledAt = DateTime.UtcNow;
    }

    public void ChangePriority(
        int priority)
    {
        if (priority < 1 ||
            priority > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                "Öncelik değeri 1 ile 100 arasında olmalıdır.");
        }

        Priority = priority;
    }

    public void SetAiHandoffSummary(
        string? summary)
    {
        AiHandoffSummary =
            NormalizeOptionalValue(
                summary);
    }

    public void SetTransferReason(
        string? reason)
    {
        TransferReason =
            NormalizeOptionalValue(
                reason);
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue &&
               ExpiresAt.Value <= DateTime.UtcNow;
    }

    private static string? NormalizeOptionalValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}