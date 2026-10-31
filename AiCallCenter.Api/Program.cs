using System.Text;
using AiCallCenter.Api.ExceptionHandling;
using AiCallCenter.Api.Hubs;
using AiCallCenter.Api.Realtime;
using AiCallCenter.Api.Security;
using AiCallCenter.Application.Common.Realtime;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.DependencyInjection;
using AiCallCenter.Infrastructure.Authentication;
using AiCallCenter.Infrastructure.DependencyInjection;
using AiCallCenter.Infrastructure.Telephony.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// DOSYA YÜKLEME SINIRI
// ----------------------------------------------------

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit =
        10 * 1024 * 1024;
});

// ----------------------------------------------------
// JWT AYARLARI
// ----------------------------------------------------

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT ayarlarý bulunamadý.");

if (string.IsNullOrWhiteSpace(
        jwtOptions.SecretKey))
{
    throw new InvalidOperationException(
        "JWT gizli anahtarý bulunamadý.");
}

// ----------------------------------------------------
// AUTHENTICATION
// ----------------------------------------------------

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidIssuer =
                    jwtOptions.Issuer,

                ValidateAudience = true,

                ValidAudience =
                    jwtOptions.Audience,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SecretKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

        options.Events =
            new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken =
                        context.Request.Query[
                            "access_token"];

                    var requestPath =
                        context.HttpContext
                            .Request.Path;

                    if (!string.IsNullOrWhiteSpace(
                            accessToken) &&
                        requestPath.StartsWithSegments(
                            "/hubs/calls"))
                    {
                        context.Token =
                            accessToken;
                    }

                    return Task.CompletedTask;
                },

                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine(
                        "JWT doðrulama hatasý: " +
                        context.Exception.Message);

                    return Task.CompletedTask;
                },

                OnChallenge = context =>
                {
                    Console.WriteLine(
                        "JWT Challenge: " +
                        context.ErrorDescription);

                    return Task.CompletedTask;
                },

                OnTokenValidated = context =>
                {
                    Console.WriteLine(
                        "JWT baþarýyla doðrulandý.");

                    return Task.CompletedTask;
                }
            };
    });

builder.Services.AddAuthorization();

// ----------------------------------------------------
// CORS
// ----------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "FrontendPolicy",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173",
                    "http://127.0.0.1:5173",
                    "http://127.0.0.1:5174")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// ----------------------------------------------------
// PROJE KATMANLARI
// ----------------------------------------------------

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

builder.Services.AddTelephony(
    builder.Configuration);

// ----------------------------------------------------
// HATA YÖNETÝMÝ
// ----------------------------------------------------

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();

// ----------------------------------------------------
// CONTROLLERS VE API
// ----------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();

// ----------------------------------------------------
// SWAGGER
// ----------------------------------------------------

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "AI Call Center API",
            Version = "v1"
        });

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type =
                SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Login iþleminden aldýðýnýz JWT tokenýný girin."
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType
                                    .SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});

// ----------------------------------------------------
// CURRENT USER
// ----------------------------------------------------

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

// ----------------------------------------------------
// SIGNALR
// ----------------------------------------------------

builder.Services.AddSignalR();

builder.Services.AddScoped<
    ICallRealtimeNotifier,
    SignalRCallRealtimeNotifier>();

// ----------------------------------------------------
// UYGULAMA
// ----------------------------------------------------

var app = builder.Build();

app.UseExceptionHandler();

// ----------------------------------------------------
// DEVELOPMENT
// ----------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options.Title =
            "AI Call Center API";

        options.Theme =
            ScalarTheme.Default;
    });

    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "AI Call Center API v1");

        options.RoutePrefix =
            "swagger";
    });
}

// ----------------------------------------------------
// HTTP PIPELINE
// ----------------------------------------------------

app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();

app.UseAuthorization();

// ----------------------------------------------------
// ENDPOINTLER
// ----------------------------------------------------

app.MapControllers();

app.MapHub<CallHub>(
    "/hubs/calls");

app.Run();