using AiCallCenter.Domain.Enums;

namespace AiCallCenter.Application.Users.DTOs;

public sealed class UserDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}
