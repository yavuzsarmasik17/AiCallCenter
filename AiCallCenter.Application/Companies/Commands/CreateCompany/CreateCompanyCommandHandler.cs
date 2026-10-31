using AiCallCenter.Application.Companies.DTOs;
using AiCallCenter.Application.Companies.Interfaces;
using AiCallCenter.Domain.Entities;
using MediatR;

namespace AiCallCenter.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyCommandHandler
    : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly ICompanyRepository _companyRepository;

    public CreateCompanyCommandHandler(
        ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<CompanyDto> Handle(
        CreateCompanyCommand request,
        CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "-");

        var slugExists = await _companyRepository.SlugExistsAsync(
            normalizedSlug,
            cancellationToken);

        if (slugExists)
        {
            throw new InvalidOperationException(
                "Bu firma kısa adı daha önce kullanılmış.");
        }

        var company = new Company(
            request.Name,
            normalizedSlug,
            request.SubscriptionPlan);

        company.UpdateContactInformation(
            request.TaxNumber,
            request.PhoneNumber,
            request.Email);

        await _companyRepository.AddAsync(
            company,
            cancellationToken);

        await _companyRepository.SaveChangesAsync(
            cancellationToken);

        return new CompanyDto
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
        };
    }
}