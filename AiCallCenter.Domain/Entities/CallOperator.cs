using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class CallOperator : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; } = string.Empty;

    public string? ExtensionNumber { get; private set; }

    public CallOperatorStatus Status { get; private set; }

    public Guid? CurrentCallId { get; private set; }

    public int MaxConcurrentCalls { get; private set; }

    public int ActiveCallCount { get; private set; }

    public bool CanReceiveCalls { get; private set; }

    public DateTime? LastActivityAt { get; private set; }

    public DateTime? LastStatusChangedAt { get; private set; }

    public Company Company { get; private set; } = null!;

    public User User { get; private set; } = null!;

    public ICollection<CallOperatorAiAgent> AiAgentAssignments
    { get; private set; } = new List<CallOperatorAiAgent>();

    public ICollection<CallQueueOperator> QueueAssignments
    { get; private set; } = new List<CallQueueOperator>();

    private CallOperator()
    {
    }

    public CallOperator(
        Guid companyId,
        Guid userId,
        string displayName,
        string? extensionNumber = null,
        int maxConcurrentCalls = 1)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "Kullanıcı kimliği geçersiz.",
                nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Operatör adı boş olamaz.",
                nameof(displayName));
        }

        if (maxConcurrentCalls <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxConcurrentCalls),
                "Eş zamanlı çağrı sayısı sıfırdan büyük olmalıdır.");
        }

        CompanyId = companyId;
        UserId = userId;
        DisplayName = displayName.Trim();
        ExtensionNumber = NormalizeOptionalValue(extensionNumber);
        Status = CallOperatorStatus.Offline;
        MaxConcurrentCalls = maxConcurrentCalls;
        ActiveCallCount = 0;
        CanReceiveCalls = true;
    }

    public void SetAvailable()
    {
        Status = CallOperatorStatus.Available;
        CurrentCallId = null;
        LastStatusChangedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void SetBusy(Guid callId)
    {
        if (callId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.",
                nameof(callId));
        }

        Status = CallOperatorStatus.Busy;
        CurrentCallId = callId;
        LastStatusChangedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void SetOnBreak()
    {
        Status = CallOperatorStatus.OnBreak;
        CurrentCallId = null;
        LastStatusChangedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void SetAfterCallWork()
    {
        Status = CallOperatorStatus.AfterCallWork;
        CurrentCallId = null;
        LastStatusChangedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void SetOffline()
    {
        Status = CallOperatorStatus.Offline;
        CurrentCallId = null;
        LastStatusChangedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
    }

    public void StartCall(Guid callId)
    {
        if (!CanAcceptCall())
        {
            throw new InvalidOperationException(
                "Operatör şu anda yeni çağrı alamaz.");
        }

        ActiveCallCount++;
        SetBusy(callId);
    }

    public void FinishCall()
    {
        if (ActiveCallCount > 0)
        {
            ActiveCallCount--;
        }

        CurrentCallId = null;

        Status = ActiveCallCount == 0
            ? CallOperatorStatus.AfterCallWork
            : CallOperatorStatus.Busy;

        LastActivityAt = DateTime.UtcNow;
        LastStatusChangedAt = DateTime.UtcNow;
    }

    public void UpdateSettings(
        string displayName,
        string? extensionNumber,
        int maxConcurrentCalls,
        bool canReceiveCalls)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException(
                "Operatör adı boş olamaz.",
                nameof(displayName));
        }

        if (maxConcurrentCalls <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxConcurrentCalls),
                "Eş zamanlı çağrı sayısı sıfırdan büyük olmalıdır.");
        }

        DisplayName = displayName.Trim();
        ExtensionNumber = NormalizeOptionalValue(extensionNumber);
        MaxConcurrentCalls = maxConcurrentCalls;
        CanReceiveCalls = canReceiveCalls;
        LastActivityAt = DateTime.UtcNow;
    }

    public bool CanAcceptCall()
    {
        return CanReceiveCalls &&
               Status == CallOperatorStatus.Available &&
               ActiveCallCount < MaxConcurrentCalls;
    }

    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}