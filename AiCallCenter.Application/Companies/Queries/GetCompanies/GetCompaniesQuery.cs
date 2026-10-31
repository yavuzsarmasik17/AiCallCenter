using AiCallCenter.Application.Companies.DTOs;
using MediatR;

namespace AiCallCenter.Application.Companies.Queries.GetCompanies;

public sealed record GetCompaniesQuery
    : IRequest<IReadOnlyList<CompanyDto>>;