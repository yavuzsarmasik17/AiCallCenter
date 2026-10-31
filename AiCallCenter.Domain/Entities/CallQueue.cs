using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class CallQueue : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid? AiAgentId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public int Priority { get; private set; }

    public int MaxWaitingSeconds { get; private set; }

    public int MaxQueueSize { get; private set; }

    public bool IsActive { get; private set; }

    public Company Company { get; private set; } = null!;

    public AiAgent? AiAgent { get; private set; }

    public ICollection<CallQueueOperator> OperatorAssignments
    { get; private set; } = new List<CallQueueOperator>();

    public ICollection<CallQueueItem> Items
    { get; private set; } = new List<CallQueueItem>();

    private CallQueue()
    {
    }

    public CallQueue(
        Guid companyId,
        string name,
        string? description,
        int priority,
        int maxWaitingSeconds,
        int maxQueueSize,
        Guid? aiAgentId = null)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        CompanyId = companyId;

        SetName(name);
        SetDescription(description);
        ChangePriority(priority);
        ChangeMaxWaitingSeconds(maxWaitingSeconds);
        ChangeMaxQueueSize(maxQueueSize);
        SetAiAgent(aiAgentId);

        IsActive = true;
    }

    public void Update(
        string name,
        string? description,
        int priority,
        int maxWaitingSeconds,
        int maxQueueSize,
        Guid? aiAgentId,
        bool isActive)
    {
        SetName(name);
        SetDescription(description);
        ChangePriority(priority);
        ChangeMaxWaitingSeconds(maxWaitingSeconds);
        ChangeMaxQueueSize(maxQueueSize);
        SetAiAgent(aiAgentId);

        IsActive = isActive;
    }

    public void SetAiAgent(Guid? aiAgentId)
    {
        if (aiAgentId.HasValue &&
            aiAgentId.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "AI ajanı kimliği geçersiz.",
                nameof(aiAgentId));
        }

        AiAgentId = aiAgentId;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Kuyruk adı boş olamaz.",
                nameof(name));
        }

        var normalizedName = name.Trim();

        if (normalizedName.Length > 150)
        {
            throw new ArgumentException(
                "Kuyruk adı en fazla 150 karakter olabilir.",
                nameof(name));
        }

        Name = normalizedName;
    }

    private void SetDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        var normalizedDescription = description.Trim();

        if (normalizedDescription.Length > 1000)
        {
            throw new ArgumentException(
                "Açıklama en fazla 1000 karakter olabilir.",
                nameof(description));
        }

        Description = normalizedDescription;
    }

    private void ChangePriority(int priority)
    {
        if (priority < 1 || priority > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(priority),
                "Öncelik 1 ile 100 arasında olmalıdır.");
        }

        Priority = priority;
    }

    private void ChangeMaxWaitingSeconds(
        int maxWaitingSeconds)
    {
        if (maxWaitingSeconds < 10 ||
            maxWaitingSeconds > 86400)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxWaitingSeconds),
                "Maksimum bekleme süresi 10 ile 86400 saniye arasında olmalıdır.");
        }

        MaxWaitingSeconds = maxWaitingSeconds;
    }

    private void ChangeMaxQueueSize(int maxQueueSize)
    {
        if (maxQueueSize < 1 ||
            maxQueueSize > 10000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxQueueSize),
                "Maksimum kuyruk kapasitesi 1 ile 10000 arasında olmalıdır.");
        }

        MaxQueueSize = maxQueueSize;
    }
}