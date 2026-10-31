namespace AiCallCenter.Application.Users.DTOs;

public sealed class UserRoleOptionDto
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
