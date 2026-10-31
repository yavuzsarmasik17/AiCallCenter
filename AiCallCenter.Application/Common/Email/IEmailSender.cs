namespace AiCallCenter.Application.Common.Email;

public interface IEmailSender
{
    Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}