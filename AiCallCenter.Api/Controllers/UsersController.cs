using AiCallCenter.Application.Common.Security;
using AiCallCenter.Application.Users.Commands.ChangeUserPassword;
using AiCallCenter.Application.Users.Commands.ChangeUserRole;
using AiCallCenter.Application.Users.Commands.CreateUser;
using AiCallCenter.Application.Users.Commands.DeleteUser;
using AiCallCenter.Application.Users.Commands.SetUserActiveStatus;
using AiCallCenter.Application.Users.Commands.UpdateUser;
using AiCallCenter.Application.Users.DTOs;
using AiCallCenter.Application.Users.Queries.GetUserById;
using AiCallCenter.Application.Users.Queries.GetUsers;
using AiCallCenter.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AiCallCenter.Api.Controllers;

[Authorize(Roles = "CompanyAdmin,SuperAdmin")]
[ApiController]
[Route("api/companies/{companyId:guid}/users")]
public sealed class UsersController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICompanyAccessValidator _companyAccessValidator;

    public UsersController(
        ISender sender,
        ICompanyAccessValidator companyAccessValidator)
    {
        _sender = sender;
        _companyAccessValidator = companyAccessValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var users = await _sender.Send(
            new GetUsersQuery(companyId),
            cancellationToken);

        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserDto>> GetById(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var user = await _sender.Send(
            new GetUserByIdQuery(companyId, userId),
            cancellationToken);

        return Ok(user);
    }

    [HttpGet("roles")]
    public ActionResult<IReadOnlyList<UserRoleOptionDto>> GetRoles(
        Guid companyId)
    {
        _companyAccessValidator.Validate(companyId);

        var roles = Enum.GetValues<UserRole>()
            .Select(role => new UserRoleOptionDto
            {
                Value = (int)role,
                Name = role.ToString(),
                DisplayName = GetRoleDisplayName(role)
            })
            .ToList();

        return Ok(roles);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create(
        Guid companyId,
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var command = new CreateUserCommand(
            companyId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Role);

        var user = await _sender.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPut("{userId:guid}")]
    public async Task<ActionResult<UserDto>> Update(
        Guid companyId,
        Guid userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var user = await _sender.Send(
            new UpdateUserCommand(
                companyId,
                userId,
                request.FirstName,
                request.LastName,
                request.Email),
            cancellationToken);

        return Ok(user);
    }

    [HttpPut("{userId:guid}/role")]
    public async Task<ActionResult<UserDto>> ChangeRole(
        Guid companyId,
        Guid userId,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var user = await _sender.Send(
            new ChangeUserRoleCommand(
                companyId,
                userId,
                request.Role),
            cancellationToken);

        return Ok(user);
    }

    [HttpPut("{userId:guid}/password")]
    public async Task<IActionResult> ChangePassword(
        Guid companyId,
        Guid userId,
        [FromBody] ChangeUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        await _sender.Send(
            new ChangeUserPasswordCommand(
                companyId,
                userId,
                request.NewPassword),
            cancellationToken);

        return NoContent();
    }

    [HttpPut("{userId:guid}/status")]
    public async Task<ActionResult<UserDto>> SetStatus(
        Guid companyId,
        Guid userId,
        [FromBody] SetUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var user = await _sender.Send(
            new SetUserActiveStatusCommand(
                companyId,
                userId,
                request.IsActive),
            cancellationToken);

        return Ok(user);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(
        Guid companyId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        _companyAccessValidator.Validate(companyId);

        var currentUserIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(currentUserIdValue, out var currentUserId))
        {
            return Unauthorized(new
            {
                message = "Oturum açan kullanıcı bilgisi alınamadı."
            });
        }

        await _sender.Send(
            new DeleteUserCommand(
                companyId,
                userId,
                currentUserId),
            cancellationToken);

        return NoContent();
    }

    private static string GetRoleDisplayName(UserRole role)
    {
        return role.ToString() switch
        {
            "SuperAdmin" => "Süper Yönetici",
            "CompanyAdmin" => "Firma Yöneticisi",
            "Manager" => "Yönetici",
            "Operator" => "Operatör",
            "Agent" => "Temsilci",
            _ => role.ToString()
        };
    }
}

public sealed class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Operator;
}

public sealed class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class ChangeUserRoleRequest
{
    public UserRole Role { get; set; }
}

public sealed class ChangeUserPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class SetUserStatusRequest
{
    public bool IsActive { get; set; }
}
