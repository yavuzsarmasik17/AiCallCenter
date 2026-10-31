namespace AiCallCenter.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; init; } = string.Empty;

    public int Port { get; init; } = 587;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string SenderEmail { get; init; } = string.Empty;

    public string SenderName { get; init; } =
        "AI Call Center";

    public bool EnableSsl { get; init; } = true;
}