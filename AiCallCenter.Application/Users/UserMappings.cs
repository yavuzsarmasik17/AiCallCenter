using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Users;

public static class UserMappings
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}