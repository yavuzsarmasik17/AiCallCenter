using System.Security.Claims;
using AiCallCenter.Application.Common.Security;

namespace AiCallCenter.Api.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?
            .User.Identity?
            .IsAuthenticated == true;

    public Guid UserId =>
        GetGuidClaim(ClaimTypes.NameIdentifier);

    public Guid CompanyId =>
        GetGuidClaim("companyId");

    public string Role =>
        _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.Role)
        ?? string.Empty;

    private Guid GetGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(claimType);

        return Guid.TryParse(value, out var result)
            ? result
            : Guid.Empty;
    }
}