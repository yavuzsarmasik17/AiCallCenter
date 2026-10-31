using AiCallCenter.Application.CallOperators.Commands
    .CreateCallOperator;
using AiCallCenter.Application.CallOperators.Commands
    .DeleteCallOperator;
using AiCallCenter.Application.CallOperators.Commands
    .UpdateCallOperator;
using AiCallCenter.Application.CallOperators.Commands
    .UpdateCallOperatorStatus;
using AiCallCenter.Application.CallOperators.Dtos;
using AiCallCenter.Application.CallOperators.Queries
    .GetCallOperatorById;
using AiCallCenter.Application.CallOperators.Queries
    .GetCallOperatorByUserId;
using AiCallCenter.Application.CallOperators.Queries
    .GetCallOperators;
using AiCallCenter.Application.Common.Security;
using AiCallCenter.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[Authorize]
[ApiController]
[Route(
    "api/companies/{companyId:guid}/call-operators")]
public sealed class CallOperatorsController
    : ControllerBase
{
    private readonly ISender _sender;

    private readonly ICompanyAccessValidator
        _companyAccessValidator;

    public CallOperatorsController(
        ISender sender,
        ICompanyAccessValidator companyAccessValidator)
    {
        _sender = sender;
        _companyAccessValidator =
            companyAccessValidator;
    }

    [Authorize(
        Roles =
            "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyList<CallOperatorDto>>>
        GetAll(
            Guid companyId,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var operators = await _sender.Send(
            new GetCallOperatorsQuery(companyId),
            cancellationToken);

        return Ok(operators);
    }

    [Authorize(
        Roles =
            "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet("{operatorId:guid}")]
    public async Task<ActionResult<CallOperatorDto>>
        GetById(
            Guid companyId,
            Guid operatorId,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var callOperator = await _sender.Send(
            new GetCallOperatorByIdQuery(
                companyId,
                operatorId),
            cancellationToken);

        return Ok(callOperator);
    }

    [Authorize(
        Roles =
            "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<CallOperatorDto>>
        GetByUserId(
            Guid companyId,
            Guid userId,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var callOperator = await _sender.Send(
            new GetCallOperatorByUserIdQuery(
                companyId,
                userId),
            cancellationToken);

        return Ok(callOperator);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPost]
    public async Task<ActionResult<CallOperatorDto>>
        Create(
            Guid companyId,
            [FromBody]
            CreateCallOperatorRequest request,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new CreateCallOperatorCommand(
            companyId,
            request.UserId,
            request.DisplayName,
            request.ExtensionNumber,
            request.MaxConcurrentCalls,
            request.CanReceiveCalls);

        var callOperator = await _sender.Send(
            command,
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            callOperator);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpPut("{operatorId:guid}")]
    public async Task<ActionResult<CallOperatorDto>>
        Update(
            Guid companyId,
            Guid operatorId,
            [FromBody]
            UpdateCallOperatorRequest request,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new UpdateCallOperatorCommand(
            companyId,
            operatorId,
            request.DisplayName,
            request.ExtensionNumber,
            request.MaxConcurrentCalls,
            request.CanReceiveCalls);

        var callOperator = await _sender.Send(
            command,
            cancellationToken);

        return Ok(callOperator);
    }

    [Authorize(
        Roles =
            "CompanyAdmin,Supervisor,Operator,SuperAdmin")]
    [HttpPatch("{operatorId:guid}/status")]
    public async Task<ActionResult<CallOperatorDto>>
        UpdateStatus(
            Guid companyId,
            Guid operatorId,
            [FromBody]
            UpdateCallOperatorStatusRequest request,
            CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command =
            new UpdateCallOperatorStatusCommand(
                companyId,
                operatorId,
                request.Status);

        var callOperator = await _sender.Send(
            command,
            cancellationToken);

        return Ok(callOperator);
    }

    [Authorize(Roles = "CompanyAdmin,SuperAdmin")]
    [HttpDelete("{operatorId:guid}")]
    public async Task<IActionResult> Delete(
        Guid companyId,
        Guid operatorId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new DeleteCallOperatorCommand(
                companyId,
                operatorId),
            cancellationToken);

        return NoContent();
    }
}

public sealed class CreateCallOperatorRequest
{
    public Guid UserId { get; set; }

    public string DisplayName { get; set; }
        = string.Empty;

    public string? ExtensionNumber { get; set; }

    public int MaxConcurrentCalls { get; set; }
        = 1;

    public bool CanReceiveCalls { get; set; }
        = true;
}

public sealed class UpdateCallOperatorRequest
{
    public string DisplayName { get; set; }
        = string.Empty;

    public string? ExtensionNumber { get; set; }

    public int MaxConcurrentCalls { get; set; }
        = 1;

    public bool CanReceiveCalls { get; set; }
        = true;
}

public sealed class UpdateCallOperatorStatusRequest
{
    public CallOperatorStatus Status { get; set; }
}
