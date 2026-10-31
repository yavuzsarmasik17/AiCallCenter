using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Companies.Commands.CreateCompany;
using AiCallCenter.Application.Companies.DTOs;
using AiCallCenter.Application.Companies.Queries.GetCompanies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUserService;

    public CompaniesController(
        ISender sender,
        ICurrentUserService currentUserService)
    {
        _sender = sender;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var companies = await _sender.Send(
            new GetCompaniesQuery(),
            cancellationToken);

        if (string.Equals(
                _currentUserService.Role,
                "SuperAdmin",
                StringComparison.OrdinalIgnoreCase))
        {
            return Ok(companies);
        }

        var ownCompanies = companies
            .Where(company =>
                company.Id == _currentUserService.CompanyId)
            .ToList();

        return Ok(ownCompanies);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<CompanyDto>> Create(
        [FromBody] CreateCompanyCommand command,
        CancellationToken cancellationToken)
    {
        var company = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            company);
    }
}