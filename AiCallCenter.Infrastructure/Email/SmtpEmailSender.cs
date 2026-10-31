using System.Net;
using System.Net.Mail;
using AiCallCenter.Application.Common.Email;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _options;

    public SmtpEmailSender(
        IOptions<SmtpOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        using var message = new MailMessage();

        message.From = new MailAddress(
            _options.SenderEmail,
            _options.SenderName);

        message.To.Add(recipientEmail);

        message.Subject = subject;
        message.Body = htmlBody;
        message.IsBodyHtml = true;

        using var smtpClient = new SmtpClient(
            _options.Host,
            _options.Port);

        smtpClient.EnableSsl = _options.EnableSsl;

        smtpClient.Credentials =
            new NetworkCredential(
                _options.Username,
                _options.Password);

        await smtpClient.SendMailAsync(
            message,
            cancellationToken);
    }
}