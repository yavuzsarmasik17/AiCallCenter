namespace AiCallCenter.Application.Authentication.DTOs;

public sealed class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;
}