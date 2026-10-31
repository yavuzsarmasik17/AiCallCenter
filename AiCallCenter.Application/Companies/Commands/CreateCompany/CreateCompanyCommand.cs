using AiCallCenter.Application.Companies.DTOs;
using AiCallCenter.Domain.Enums;
using MediatR;

namespace AiCallCenter.Application.Companies.Commands.CreateCompany;

public sealed record CreateCompanyCommand(
    string Name,
    string Slug,
    string? TaxNumber,
    string? PhoneNumber,
    string? Email,
    SubscriptionPlan SubscriptionPlan)
    : IRequest<CompanyDto>;