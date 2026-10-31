using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class CallNote : AuditableEntity
{
    public Guid CompanyId { get; private set; }

    public Guid CallId { get; private set; }

    public Guid OperatorId { get; private set; }

    public string Note { get; private set; } = string.Empty;

    public DateTime CreatedAtUtc { get; private set; }

    public Company Company { get; private set; } = null!;

    public Call Call { get; private set; } = null!;

    public CallOperator Operator { get; private set; } = null!;

    private CallNote()
    {
    }

    public CallNote(
        Guid companyId,
        Guid callId,
        Guid operatorId,
        string note)
    {
        CompanyId = companyId;
        CallId = callId;
        OperatorId = operatorId;
        Note = note.Trim();
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string note)
    {
        Note = note.Trim();
    }
}