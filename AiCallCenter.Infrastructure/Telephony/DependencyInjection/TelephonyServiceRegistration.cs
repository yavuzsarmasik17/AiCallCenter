using AiCallCenter.Application.Common.Telephony;
using AiCallCenter.Infrastructure.Telephony.Mock;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AiCallCenter.Infrastructure.Telephony
    .DependencyInjection;

public static class TelephonyServiceRegistration
{
    public static IServiceCollection AddTelephony(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TelephonyOptions>(
            configuration.GetSection(
                TelephonyOptions.SectionName));

        services.AddSingleton<
            MockTelephonyProvider>();

        services.AddSingleton<
            ITelephonyProvider>(
                serviceProvider =>
                    serviceProvider.GetRequiredService<
                        MockTelephonyProvider>());

        services.AddSingleton<
            ITelephonyProviderResolver,
            TelephonyProviderResolver>();

        return services;
    }
}
