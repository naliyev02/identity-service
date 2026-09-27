using Identity.API.Contracts.Requests;
using Identity.API.Contracts.Responses;
using Identity.API.Cookies;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Features.Authentication.Commands.ResendVerification;
using Identity.Application.Features.Authentication.Commands.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _environment;

    public AuthController(ISender sender, IWebHostEnvironment environment)
    {
        _sender = sender;
        _environment = environment;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AccessTokenResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ErrorCode == "AccountCannotSignIn")
                return StatusCode(StatusCodes.Status403Forbidden, new { error = result.ErrorCode });

            return Unauthorized();
        }

        AppendRefreshCookie(result.RefreshToken!, result.RefreshTokenExpiresAtUtc!.Value);
        return Ok(new AccessTokenResponse(result.AccessToken!, result.AccessTokenExpiresAtUtc!.Value));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AccessTokenResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(RefreshTokenCookie.Name, out var refreshToken))
            return Unauthorized();

        var result = await _sender.Send(new RefreshTokenCommand(refreshToken), cancellationToken);
        if (!result.Succeeded)
        {
            DeleteRefreshCookie();
            return Unauthorized();
        }

        AppendRefreshCookie(result.RefreshToken!, result.RefreshTokenExpiresAtUtc!.Value);
        return Ok(new AccessTokenResponse(result.AccessToken!, result.AccessTokenExpiresAtUtc!.Value));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(RefreshTokenCookie.Name, out var refreshToken);
        await _sender.Send(new LogoutCommand(refreshToken), cancellationToken);
        DeleteRefreshCookie();
        return NoContent();
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult<VerifyEmailResponse>> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new VerifyEmailCommand(request.Token), cancellationToken);

        return Ok(new VerifyEmailResponse(
            result.Id,
            result.Email,
            result.FirstName,
            result.LastName,
            result.State.ToString()));
    }

    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ResendVerificationCommand(request.Email), cancellationToken);
        return Accepted();
    }

    private void AppendRefreshCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(
            RefreshTokenCookie.Name,
            refreshToken,
            RefreshTokenCookie.Create(
                _environment.IsDevelopment(),
                new DateTimeOffset(DateTime.SpecifyKind(expiresAtUtc, DateTimeKind.Utc))));
    }

    private void DeleteRefreshCookie()
        => Response.Cookies.Delete(
            RefreshTokenCookie.Name,
            RefreshTokenCookie.Create(_environment.IsDevelopment()));
}
