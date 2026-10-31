namespace AiCallCenter.Application.Common.Security;

public interface ICompanyAccessValidator
{
    void Validate(Guid requestedCompanyId);
}