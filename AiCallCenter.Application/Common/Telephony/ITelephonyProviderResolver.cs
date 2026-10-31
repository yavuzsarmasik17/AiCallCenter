namespace AiCallCenter.Application.Common.Telephony;

public interface ITelephonyProviderResolver
{
    ITelephonyProvider Resolve();

    ITelephonyProvider Resolve(string providerName);
}
