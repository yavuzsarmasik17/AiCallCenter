using AiCallCenter.Application.Authentication.Commands.ForgotPassword;
using AiCallCenter.Application.Authentication.Commands.ResetPassword;
using AiCallCenter.Application.Authentication.Commands.VerifyResetCode;
using AiCallCenter.Application.Authentication.Commands.Login;
using AiCallCenter.Application.Authentication.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AiCallCenter.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _sender.Send(
            new LoginCommand(
                request.Email,
                request.Password),
            cancellationToken);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordRequest request,
    CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ForgotPasswordCommand(request.Email),
            cancellationToken);

        return Ok(new
        {
            Message =
                "Eğer e-posta adresi sistemde kayıtlıysa doğrulama kodu gönderilecektir."
        });
    }

    [AllowAnonymous]
    [HttpPost("verify-reset-code")]
    public async Task<IActionResult> VerifyResetCode(
        [FromBody] VerifyResetCodeRequest request,
        CancellationToken cancellationToken)
    {
        var isValid = await _sender.Send(
            new VerifyResetCodeCommand(
                request.Email,
                request.Code),
            cancellationToken);

        if (!isValid)
        {
            return BadRequest(new
            {
                Message =
                    "Doğrulama kodu geçersiz, süresi dolmuş veya deneme sınırı aşılmış."
            });
        }

        return Ok(new
        {
            IsValid = true,
            Message = "Doğrulama kodu doğru."
        });
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResetPasswordCommand(
                request.Email,
                request.Code,
                request.NewPassword,
                request.ConfirmPassword),
            cancellationToken);

        return Ok(new
        {
            Message =
                "Şifreniz başarıyla değiştirildi. Yeni şifrenizle giriş yapabilirsiniz."
        });
    }





}



public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}