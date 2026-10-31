using AiCallCenter.Domain.Entities;

namespace AiCallCenter.Application.Common.Security;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}