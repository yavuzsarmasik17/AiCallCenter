namespace AiCallCenter.Application.Authentication.DTOs;

public sealed class JwtExpirationOptions
{
    public int ExpirationMinutes { get; set; } = 60;
}