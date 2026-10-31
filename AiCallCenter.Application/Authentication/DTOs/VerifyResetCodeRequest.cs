namespace AiCallCenter.Application.Authentication.DTOs;

public sealed class VerifyResetCodeRequest
{
    public string Email { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
}