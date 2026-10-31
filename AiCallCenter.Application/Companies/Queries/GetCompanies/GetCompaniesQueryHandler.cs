using AiCallCenter.Application.Companies.DTOs;
using AiCallCenter.Application.Companies.Interfaces;
using MediatR;

namespace AiCallCenter.Application.Companies.Queries.GetCompanies;

public sealed class GetCompaniesQueryHandler
    : IRequestHandler<GetCompaniesQuery, IReadOnlyList<CompanyDto>>
{
    private readonly ICompanyRepository _companyRepository;

    public GetCompaniesQueryHandler(
        ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IReadOnlyList<CompanyDto>> Handle(
        GetCompaniesQuery request,
        CancellationToken cancellationToken)
    {
        var companies = await _companyRepository.GetAllAsync(
            cancellationToken);

        return companies
            .Select(company => new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Slug = company.Slug,
                TaxNumber = company.TaxNumber,
                PhoneNumber = company.PhoneNumber,
                Email = company.Email,
                SubscriptionPlan = company.SubscriptionPlan,
                MaxUserCount = company.MaxUserCount,
                MaxAiAgentCount = company.MaxAiAgentCount,
                IsActive = company.IsActive,
                CreatedAt = company.CreatedAt
            })
            .ToList();
    }
}