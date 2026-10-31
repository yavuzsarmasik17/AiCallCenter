using AiCallCenter.Application.Common.Telephony;
using Microsoft.Extensions.Options;

namespace AiCallCenter.Infrastructure.Telephony;

public sealed class TelephonyProviderResolver
    : ITelephonyProviderResolver
{
    private readonly IReadOnlyDictionary<
        string,
        ITelephonyProvider> _providers;

    private readonly TelephonyOptions _options;

    public TelephonyProviderResolver(
        IEnumerable<ITelephonyProvider> providers,
        IOptions<TelephonyOptions> options)
    {
        _options = options.Value;

        _providers = providers.ToDictionary(
            provider => provider.Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public ITelephonyProvider Resolve()
    {
        return Resolve(_options.Provider);
    }

    public ITelephonyProvider Resolve(
        string providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            throw new InvalidOperationException(
                "Telefon sağlayıcısı belirtilmedi.");
        }

        if (_providers.TryGetValue(
            providerName.Trim(),
            out var provider))
        {
            return provider;
        }

        var availableProviders =
            string.Join(", ", _providers.Keys);

        throw new InvalidOperationException(
            $"'{providerName}' telefon sağlayıcısı kayıtlı değil. " +
            $"Kayıtlı sağlayıcılar: {availableProviders}");
    }
}
