using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Authentication.DTOs;

public sealed class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime ExpiresAt { get; set; }

    public Guid UserId { get; set; }

    public Guid CompanyId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}