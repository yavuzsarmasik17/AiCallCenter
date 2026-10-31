using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "İstek işlenirken hata oluştu. Path: {Path}",
            httpContext.Request.Path);

        var problemDetails = CreateProblemDetails(
            httpContext,
            exception);

        httpContext.Response.StatusCode =
            problemDetails.Status
            ?? StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(
    problemDetails,
    CancellationToken.None);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    "Giriş başarısız.",
                    exception.Message),

            ValidationException validationException =>
                CreateValidationProblemDetails(
                    httpContext,
                    validationException),

            KeyNotFoundException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Kayıt bulunamadı.",
                    exception.Message),

            InvalidOperationException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "İşlem gerçekleştirilemedi.",
                    exception.Message),

            ArgumentException =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Geçersiz istek.",
                    exception.Message),

            _ =>
                CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Sunucu hatası.",
                    "Beklenmeyen bir hata oluştu.")
        };
    }

    private static ValidationProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        ValidationException exception)
    {
        var errors = exception.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Doğrulama hatası.",
            Detail = "Gönderilen bilgiler geçersiz.",
            Instance = httpContext.Request.Path
        };
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext httpContext,
        int statusCode,
        string title,
        string detail)
    {
        return new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
    }
}