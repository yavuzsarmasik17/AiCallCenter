using AiCallCenter.Application.Common.Telephony;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/telephony/webhooks")]
public sealed class TelephonyWebhookController
    : ControllerBase
{
    private readonly ILogger<TelephonyWebhookController>
        _logger;

    public TelephonyWebhookController(
        ILogger<TelephonyWebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost("{provider}/status")]
    public IActionResult ReceiveStatus(
        string provider,
        [FromBody]
        TelephonyWebhookEvent webhookEvent)
    {
        if (!string.Equals(
            provider,
            webhookEvent.Provider,
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(
                new
                {
                    message =
                        "URL sağlayıcısı ile içerikteki " +
                        "sağlayıcı eşleşmiyor."
                });
        }

        _logger.LogInformation(
            "Telefon webhook alındı. Provider: {Provider}, " +
            "ProviderCallId: {ProviderCallId}, Status: {Status}",
            webhookEvent.Provider,
            webhookEvent.ProviderCallId,
            webhookEvent.Status);

        /*
         * İkinci aşamada burada:
         * 1. Webhook imzası doğrulanacak.
         * 2. ProviderCallId ile Call kaydı bulunacak.
         * 3. Call durumu güncellenecek.
         * 4. CallEvent kaydı eklenecek.
         * 5. SignalR ile frontend bilgilendirilecek.
         */

        return Ok(new
        {
            received = true
        });
    }
}
