using System.Collections.Concurrent;
using AiCallCenter.Application.Common.Telephony;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.Telephony.Mock;

public sealed class MockTelephonyProvider
    : ITelephonyProvider
{
    private readonly ConcurrentDictionary<
        string,
        MockTelephonyCallState> _calls = new();

    private readonly TelephonyOptions _options;

    private readonly ILogger<MockTelephonyProvider>
        _logger;

    public MockTelephonyProvider(
        IOptions<TelephonyOptions> options,
        ILogger<MockTelephonyProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public string Name => TelephonyProviderName.Mock;

    public async Task<TelephonyCallResult>
        StartOutboundCallAsync(
            StartOutboundCallRequest request,
            CancellationToken cancellationToken = default)
    {
        ValidateStartRequest(request);

        var providerCallId =
            $"MOCK-{Guid.NewGuid():N}".ToUpperInvariant();

        var fromNumber =
            string.IsNullOrWhiteSpace(request.FromNumber)
                ? _options.DefaultFromNumber
                : request.FromNumber.Trim();

        var now = DateTime.UtcNow;

        var state = new MockTelephonyCallState
        {
            CompanyId = request.CompanyId,
            CallId = request.CallId,
            ProviderCallId = providerCallId,
            FromNumber = fromNumber,
            ToNumber = request.ToNumber.Trim(),
            Status = TelephonyCallStatus.Queued,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _calls[providerCallId] = state;

        _logger.LogInformation(
            "Mock çağrı oluşturuldu. CallId: {CallId}, " +
            "ProviderCallId: {ProviderCallId}, To: {ToNumber}",
            request.CallId,
            providerCallId,
            state.ToNumber);

        await Task.Delay(
            TimeSpan.FromSeconds(
                Math.Max(
                    0,
                    _options.Mock.RingingDelaySeconds)),
            cancellationToken);

        state.Status = TelephonyCallStatus.Ringing;
        state.UpdatedAtUtc = DateTime.UtcNow;

        if (_options.Mock.AutoComplete)
        {
            _ = AutoCompleteAsync(
                providerCallId,
                _options.Mock.AutoCompleteDelaySeconds);
        }

        return TelephonyCallResult.Success(
            Name,
            providerCallId,
            state.Status);
    }

    public Task<TelephonyCallResult>
        TransferCallAsync(
            TransferTelephonyCallRequest request,
            CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ProviderCallId))
        {
            return Task.FromResult(
                TelephonyCallResult.Failure(
                    Name,
                    "PROVIDER_CALL_ID_REQUIRED",
                    "Sağlayıcı çağrı kimliği zorunludur."));
        }

        if (string.IsNullOrWhiteSpace(request.Target))
        {
            return Task.FromResult(
                TelephonyCallResult.Failure(
                    Name,
                    "TRANSFER_TARGET_REQUIRED",
                    "Aktarım hedefi zorunludur."));
        }

        if (!_calls.TryGetValue(
            request.ProviderCallId,
            out var state))
        {
            return Task.FromResult(
                TelephonyCallResult.Failure(
                    Name,
                    "CALL_NOT_FOUND",
                    "Mock çağrı kaydı bulunamadı."));
        }

        state.Status = TelephonyCallStatus.InProgress;
        state.UpdatedAtUtc = DateTime.UtcNow;

        _logger.LogInformation(
            "Mock çağrı aktarıldı. ProviderCallId: " +
            "{ProviderCallId}, Target: {Target}",
            request.ProviderCallId,
            request.Target);

        return Task.FromResult(
            TelephonyCallResult.Success(
                Name,
                state.ProviderCallId,
                state.Status));
    }

    public Task<TelephonyCallResult>
        HangupCallAsync(
            Guid companyId,
            Guid callId,
            string providerCallId,
            CancellationToken cancellationToken = default)
    {
        if (!_calls.TryGetValue(
            providerCallId,
            out var state))
        {
            return Task.FromResult(
                TelephonyCallResult.Failure(
                    Name,
                    "CALL_NOT_FOUND",
                    "Mock çağrı kaydı bulunamadı."));
        }

        if (state.CompanyId != companyId ||
            state.CallId != callId)
        {
            return Task.FromResult(
                TelephonyCallResult.Failure(
                    Name,
                    "CALL_MISMATCH",
                    "Çağrı firma veya kayıt bilgisi eşleşmiyor."));
        }

        state.Status = TelephonyCallStatus.Completed;
        state.UpdatedAtUtc = DateTime.UtcNow;

        _logger.LogInformation(
            "Mock çağrı sonlandırıldı. ProviderCallId: " +
            "{ProviderCallId}",
            providerCallId);

        return Task.FromResult(
            TelephonyCallResult.Success(
                Name,
                providerCallId,
                state.Status));
    }

    private async Task AutoCompleteAsync(
        string providerCallId,
        int delaySeconds)
    {
        try
        {
            await Task.Delay(
                TimeSpan.FromSeconds(
                    Math.Max(1, delaySeconds)));

            if (_calls.TryGetValue(
                providerCallId,
                out var state) &&
                state.Status != TelephonyCallStatus.Completed)
            {
                state.Status =
                    TelephonyCallStatus.Completed;

                state.UpdatedAtUtc =
                    DateTime.UtcNow;

                _logger.LogInformation(
                    "Mock çağrı otomatik tamamlandı. " +
                    "ProviderCallId: {ProviderCallId}",
                    providerCallId);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Mock çağrı otomatik tamamlama hatası.");
        }
    }

    private static void ValidateStartRequest(
        StartOutboundCallRequest request)
    {
        if (request.CompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.");
        }

        if (request.CallId == Guid.Empty)
        {
            throw new ArgumentException(
                "Çağrı kimliği geçersiz.");
        }

        if (string.IsNullOrWhiteSpace(
            request.ToNumber))
        {
            throw new ArgumentException(
                "Aranacak telefon numarası zorunludur.");
        }
    }
}
