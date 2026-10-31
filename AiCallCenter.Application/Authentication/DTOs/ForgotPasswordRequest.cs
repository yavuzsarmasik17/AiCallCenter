namespace AiCallCenter.Application.Authentication.DTOs;

public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}