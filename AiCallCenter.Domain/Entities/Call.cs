using AiCallCenter.Domain.Common;
using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Domain.Entities;

public sealed class Call : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid AiAgentId { get; private set; }

    public string? ProviderCallId { get; private set; }

    public string CallerNumber { get; private set; } = string.Empty;

    public string CalledNumber { get; private set; } = string.Empty;

    public CallDirection Direction { get; private set; }

    public CallStatus Status { get; private set; }

    public DateTime StartedAt { get; private set; }

    public DateTime? AnsweredAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public int DurationSeconds { get; private set; }

    public string? Summary { get; private set; }

    public string? Result { get; private set; }

    public bool TransferredToHuman { get; private set; }

    public string? TransferNumber { get; private set; }

    public Company Company { get; private set; } = null!;

    public AiAgent AiAgent { get; private set; } = null!;

    public ICollection<CallMessage> Messages { get; private set; }
        = new List<CallMessage>();

    public ICollection<CallEvent> Events { get; private set; }
        = new List<CallEvent>();

    private Call()
    {
    }

    public Call(
        Guid companyId,
        Guid aiAgentId,
        string callerNumber,
        string calledNumber,
        CallDirection direction)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(companyId));
        }

        if (aiAgentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Yapay zekâ ajanı kimliği geçersiz.",
                nameof(aiAgentId));
        }

        CompanyId = companyId;
        AiAgentId = aiAgentId;

        CallerNumber = NormalizePhoneNumber(
            callerNumber,
            nameof(callerNumber));

        CalledNumber = NormalizePhoneNumber(
            calledNumber,
            nameof(calledNumber));

        Direction = direction;
        Status = CallStatus.Created;
        StartedAt = DateTime.UtcNow;
        DurationSeconds = 0;
        TransferredToHuman = false;
    }

    public void SetProviderCallId(string providerCallId)
    {
        if (string.IsNullOrWhiteSpace(providerCallId))
        {
            throw new ArgumentException(
                "Sağlayıcı çağrı kimliği boş olamaz.",
                nameof(providerCallId));
        }

        ProviderCallId = providerCallId.Trim();
    }

    public void MarkAsRinging()
    {
        Status = CallStatus.Ringing;
    }

    public void Start()
    {
        if (Status == CallStatus.Completed ||
            Status == CallStatus.Cancelled ||
            Status == CallStatus.Failed)
        {
            throw new InvalidOperationException(
                "Tamamlanmış veya iptal edilmiş çağrı başlatılamaz.");
        }

        Status = CallStatus.InProgress;
        AnsweredAt = DateTime.UtcNow;
    }

    public void Complete(
        string? summary = null,
        string? result = null)
    {
        if (Status == CallStatus.Completed)
        {
            return;
        }

        EndedAt = DateTime.UtcNow;
        Status = CallStatus.Completed;

        Summary = NormalizeOptionalValue(summary);
        Result = NormalizeOptionalValue(result);

        CalculateDuration();
    }

    public void Fail(string? result = null)
    {
        EndedAt = DateTime.UtcNow;
        Status = CallStatus.Failed;
        Result = NormalizeOptionalValue(result);

        CalculateDuration();
    }

    public void Cancel(string? result = null)
    {
        EndedAt = DateTime.UtcNow;
        Status = CallStatus.Cancelled;
        Result = NormalizeOptionalValue(result);

        CalculateDuration();
    }

    // Gerçek telefon sağlayıcısı üzerinden belirli bir
    // numara/extension'a aktarım yapılacağı zaman kullanılır.
    public void TransferToHumanAgent(string transferNumber)
    {
        if (string.IsNullOrWhiteSpace(transferNumber))
        {
            throw new ArgumentException(
                "Aktarım numarası boş olamaz.",
                nameof(transferNumber));
        }

        TransferredToHuman = true;
        TransferNumber = transferNumber.Trim();
        Status = CallStatus.Transferred;
    }

    // Uygulama içindeki çağrı kuyruğuna aktarım yapılacağı
    // zaman kullanılır. Telefon numarası gerektirmez.
    public void TransferToHumanQueue()
    {
        if (Status == CallStatus.Completed ||
            Status == CallStatus.Cancelled ||
            Status == CallStatus.Failed)
        {
            throw new InvalidOperationException(
                "Tamamlanmış, iptal edilmiş veya başarısız çağrı aktarılamaz.");
        }

        TransferredToHuman = true;
        TransferNumber = null;
        Status = CallStatus.Transferred;
    }

    public void UpdateSummary(
        string? summary,
        string? result)
    {
        Summary = NormalizeOptionalValue(summary);
        Result = NormalizeOptionalValue(result);
    }

    private void CalculateDuration()
    {
        var startTime = AnsweredAt ?? StartedAt;
        var endTime = EndedAt ?? DateTime.UtcNow;

        DurationSeconds = Math.Max(
            0,
            (int)(endTime - startTime).TotalSeconds);
    }

    private static string NormalizePhoneNumber(
        string phoneNumber,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            throw new ArgumentException(
                "Telefon numarası boş olamaz.",
                parameterName);
        }

        var normalized = phoneNumber.Trim();

        if (normalized.Length > 30)
        {
            throw new ArgumentException(
                "Telefon numarası en fazla 30 karakter olabilir.",
                parameterName);
        }

        return normalized;
    }

    private static string? NormalizeOptionalValue(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}