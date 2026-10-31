using AiCallCenter.Application.Calls.Interfaces;
using AiCallCenter.Infrastructure.Persistence.Repositories;
using AiCallCenter.Application.CallQueueItems.Interfaces;
using AiCallCenter.Application.CallRouting.Interfaces;
using AiCallCenter.Infrastructure.Repositories;
using AiCallCenter.Application.CallQueues.Interfaces;
using AiCallCenter.Application.CallOperatorAiAgents.Interfaces;
using AiCallCenter.Application.Authentication.Interfaces;
using AiCallCenter.Application.Common.PasswordReset;
using AiCallCenter.Infrastructure.PasswordReset;
using AiCallCenter.Application.Common.Email;
using AiCallCenter.Infrastructure.Email;
using AiCallCenter.Application.Common.Dashboard;
using AiCallCenter.Infrastructure.Dashboard;
using System.Net.Http.Headers;
using AiCallCenter.Application.AiAgents.Interfaces;
using AiCallCenter.Application.AiConversations.Interfaces;
using AiCallCenter.Application.Authentication.DTOs;
using AiCallCenter.Application.Common.AI;
using AiCallCenter.Application.Common.AI.Transfer;
using AiCallCenter.Application.Common.AI.Handoff;
using AiCallCenter.Application.Common.Documents;
using AiCallCenter.Application.Common.Knowledge;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Common.TextProcessing;
using AiCallCenter.Application.Companies.Interfaces;
using AiCallCenter.Application.KnowledgeBases.Interfaces;
using AiCallCenter.Application.KnowledgeDocuments.Interfaces;
using AiCallCenter.Application.Users.Interfaces;
using AiCallCenter.Infrastructure.AI;
using AiCallCenter.Infrastructure.AI.Transfer;
using AiCallCenter.Infrastructure.AI.Handoff;
using AiCallCenter.Infrastructure.Authentication;
using AiCallCenter.Infrastructure.Documents;
using AiCallCenter.Infrastructure.Persistence;
using AiCallCenter.Infrastructure.Search;
using AiCallCenter.Infrastructure.Security;
using AiCallCenter.Infrastructure.TextProcessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AiCallCenter.Application.CallOperators.Interfaces;

namespace AiCallCenter.Infrastructure.DependencyInjection;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("SqlServer");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "SqlServer bağlantı cümlesi bulunamadı.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(
                connectionString,
                sqlServerOptions =>
                {
                    sqlServerOptions.MigrationsAssembly(
                        typeof(ApplicationDbContext)
                            .Assembly
                            .FullName);
                });
        });

        // -------------------------------------------------
        // Repository kayıtları
        // -------------------------------------------------

        services.AddScoped<
            ICompanyRepository,
            CompanyRepository>();

        services.AddScoped<
            ICallOperatorRepository,
            CallOperatorRepository>();

        services.AddScoped<
            ICallOperatorAiAgentRepository,
            CallOperatorAiAgentRepository>();

        services.AddScoped<
            ICallRoutingRepository,
            CallRoutingRepository>();

        services.AddScoped<
            IDashboardReadService,
            DashboardReadService>();

        services.AddScoped<
            IAiAgentRepository,
            AiAgentRepository>();

        services.AddScoped<
            IAiConversationRepository,
            AiConversationRepository>();

        services.AddScoped<
            IKnowledgeBaseRepository,
            KnowledgeBaseRepository>();

        services.AddScoped<
            IKnowledgeDocumentRepository,
            KnowledgeDocumentRepository>();

        services.AddScoped<
            IAiAgentKnowledgeBaseRepository,
            AiAgentKnowledgeBaseRepository>();

        services.AddScoped<
            ICallQueueRepository,
            CallQueueRepository>();

        services.AddScoped<
            ICallQueueOperatorRepository,
            CallQueueOperatorRepository>();

        services.AddScoped<
            ICallQueueItemRepository,
            CallQueueItemRepository>();

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddScoped<
            IPasswordResetCodeRepository,
            PasswordResetCodeRepository>();

        services.AddScoped<
            ICallRepository,
            CallRepository>();

        services.AddScoped<
            ICallMessageRepository,
            CallMessageRepository>();

        // -------------------------------------------------
        // AI Model Kataloğu
        // -------------------------------------------------

        services.AddSingleton<
            IAiModelCatalog,
            AiModelCatalog>();

        // -------------------------------------------------
        // Metin ve belge servisleri
        // -------------------------------------------------

        services.AddSingleton<
            ITextChunker,
            TextChunker>();

        services.AddScoped<
            IDocumentTextExtractor,
            PdfTextExtractor>();

        services.AddScoped<
            IKnowledgeSearchService,
            KnowledgeSearchService>();

        // -------------------------------------------------
        // Güvenlik servisleri
        // -------------------------------------------------

        services.AddSingleton<
            IPasswordHasher,
            PasswordHasher>();

        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        services.Configure<JwtExpirationOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        services.AddSingleton<
            IJwtTokenGenerator,
            JwtTokenGenerator>();

        // -------------------------------------------------
        // OpenAI ayarları
        // -------------------------------------------------

        services.Configure<OpenAiOptions>(
            configuration.GetSection(
                OpenAiOptions.SectionName));

        // -------------------------------------------------
        // Çoklu AI sağlayıcı ayarları
        // -------------------------------------------------

        services.Configure<AiProviderOptions>(
            configuration.GetSection(
                AiProviderOptions.SectionName));

        var openAiOptions =
            configuration
                .GetSection(
                    OpenAiOptions.SectionName)
                .Get<OpenAiOptions>()
            ?? new OpenAiOptions();

        var aiProviderOptions =
            configuration
                .GetSection(
                    AiProviderOptions.SectionName)
                .Get<AiProviderOptions>()
            ?? new AiProviderOptions();

        // -------------------------------------------------
        // OpenAI Provider
        // -------------------------------------------------

        services.AddHttpClient<
            OpenAiService>(client =>
            {
                client.BaseAddress =
                    new Uri(
                        openAiOptions.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        openAiOptions.TimeoutSeconds);

                ConfigureAuthorizationHeader(
                    client,
                    openAiOptions);
            });

        services.AddTransient<
            IOpenAiService>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        OpenAiService>());

        services.AddTransient<
            IAiProviderClient>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        OpenAiService>());

        // -------------------------------------------------
        // Gemini Provider
        // -------------------------------------------------

        services.AddHttpClient<
            GeminiProviderClient>(client =>
            {
                var settings =
                    aiProviderOptions.Gemini;

                client.BaseAddress =
                    new Uri(
                        settings.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        settings.TimeoutSeconds);

                ConfigureProviderAuthorizationHeader(
                    client,
                    settings);
            });

        // -------------------------------------------------
        // Grok Provider
        // -------------------------------------------------

        services.AddHttpClient<
            GrokProviderClient>(client =>
            {
                var settings =
                    aiProviderOptions.Grok;

                client.BaseAddress =
                    new Uri(
                        settings.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        settings.TimeoutSeconds);

                ConfigureProviderAuthorizationHeader(
                    client,
                    settings);
            });

        services.AddTransient<
            IAiProviderClient>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        GrokProviderClient>());

        // -------------------------------------------------
        // DeepSeek Provider
        // -------------------------------------------------

        services.AddHttpClient<
            DeepSeekProviderClient>(client =>
            {
                var settings =
                    aiProviderOptions.DeepSeek;

                client.BaseAddress =
                    new Uri(
                        settings.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        settings.TimeoutSeconds);

                ConfigureProviderAuthorizationHeader(
                    client,
                    settings);
            });

        services.AddTransient<
            IAiProviderClient>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        DeepSeekProviderClient>());

        services.AddTransient<
            IAiProviderClient>(
            serviceProvider =>
                serviceProvider
                    .GetRequiredService<
                        GeminiProviderClient>());

        // -------------------------------------------------
        // Ortak AI yönlendirme servisi
        // -------------------------------------------------

        services.AddTransient<
            IAiChatService,
            AiChatService>();

        // -------------------------------------------------
        // AI -> İnsan Operatör Transfer Karar Servisi
        // -------------------------------------------------

        services.AddTransient<
            IAiTransferDecisionService,
            MockAiTransferDecisionService>();

        // -------------------------------------------------
        // AI -> İnsan Operatör Devir Özeti Servisi
        // -------------------------------------------------

        services.AddTransient<
            IAiHandoffSummaryService,
            MockAiHandoffSummaryService>();

        // -------------------------------------------------
        // Embedding servisi
        // -------------------------------------------------

        services.AddHttpClient<
            IEmbeddingService,
            OpenAiEmbeddingService>(client =>
            {
                client.BaseAddress =
                    new Uri(
                        openAiOptions.BaseUrl);

                client.Timeout =
                    TimeSpan.FromSeconds(
                        openAiOptions.TimeoutSeconds);

                ConfigureAuthorizationHeader(
                    client,
                    openAiOptions);
            });

        // -------------------------------------------------
        // E-posta
        // -------------------------------------------------

        services.Configure<SmtpOptions>(
            configuration.GetSection(
                SmtpOptions.SectionName));

        services.AddScoped<
            IEmailSender,
            SmtpEmailSender>();

        // -------------------------------------------------
        // Şifre sıfırlama
        // -------------------------------------------------

        services.AddSingleton<
            IPasswordResetCodeGenerator,
            PasswordResetCodeGenerator>();

        return services;
    }

    private static void ConfigureAuthorizationHeader(
        HttpClient client,
        OpenAiOptions openAiOptions)
    {
        if (openAiOptions.UseMockService)
        {
            return;
        }

        var apiKey =
            Environment.GetEnvironmentVariable(
                "OPENAI_API_KEY");

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OPENAI_API_KEY ortam değişkeni bulunamadı.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);
    }

    private static void ConfigureProviderAuthorizationHeader(
        HttpClient client,
        ProviderSettings settings)
    {
        if (settings.UseMockService)
        {
            return;
        }

        var apiKey =
            Environment.GetEnvironmentVariable(
                settings.ApiKeyEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"{settings.ApiKeyEnvironmentVariable} " +
                "ortam değişkeni bulunamadı.");
        }

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                apiKey);
    }
}