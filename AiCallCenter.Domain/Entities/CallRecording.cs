using AiCallCenter.Domain.Common;

namespace AiCallCenter.Domain.Entities;

public sealed class CallRecording : AuditableEntity
{
    public Guid CallId { get; private set; }

    public string FileName { get; private set; } = string.Empty;

    public string FileUrl { get; private set; } = string.Empty;

    public int DurationSeconds { get; private set; }

    public long FileSize { get; private set; }

    public string? Transcript { get; private set; }

    public string? AiSummary { get; private set; }

    public Call Call { get; private set; } = null!;

    private CallRecording()
    {
    }

    public CallRecording(
        Guid callId,
        string fileName,
        string fileUrl)
    {
        CallId = callId;
        FileName = fileName.Trim();
        FileUrl = fileUrl.Trim();
    }

    public void SetTranscript(string transcript)
    {
        Transcript = transcript;
    }

    public void SetAiSummary(string summary)
    {
        AiSummary = summary;
    }
}