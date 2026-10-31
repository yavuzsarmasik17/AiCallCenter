namespace AiCallCenter.Application.Common.Security;

public sealed class CompanyAccessValidator
    : ICompanyAccessValidator
{
    private readonly ICurrentUserService _currentUserService;

    public CompanyAccessValidator(
        ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public void Validate(Guid requestedCompanyId)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            throw new UnauthorizedAccessException(
                "Kullanıcı oturumu bulunamadı.");
        }

        if (requestedCompanyId == Guid.Empty)
        {
            throw new ArgumentException(
                "Firma kimliği geçersiz.",
                nameof(requestedCompanyId));
        }

        // SuperAdmin bütün firmalara erişebilir.
        if (string.Equals(
                _currentUserService.Role,
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (_currentUserService.CompanyId != requestedCompanyId)
        {
            throw new UnauthorizedAccessException(
                "Bu firmaya erişim yetkiniz yok.");
        }
    }
}