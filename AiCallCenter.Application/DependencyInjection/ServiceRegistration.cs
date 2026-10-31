using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AiCallCenter.Application.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        var assembly = typeof(ServiceRegistration).Assembly;

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddScoped<
        ICompanyAccessValidator,
        CompanyAccessValidator>();

        return services;
    }

}