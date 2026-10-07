using Identity.API.Contracts.Requests;
using Identity.API.Contracts.Responses;
using Identity.API.Cookies;
using Identity.API.Extensions;
using Identity.API.RateLimiting;
using Identity.Application.Features.Authentication.Commands.ChangePassword;
using Identity.Application.Features.Authentication.Commands.ForgotPassword;
using Identity.Application.Features.Authentication.Commands.Login;
using Identity.Application.Features.Authentication.Commands.Logout;
using Identity.Application.Features.Authentication.Commands.RefreshToken;
using Identity.Application.Features.Authentication.Commands.ResendVerification;
using Identity.Application.Features.Authentication.Commands.ResetPassword;
using Identity.Application.Features.Authentication.Commands.VerifyEmail;
using Identity.Application.Features.Authentication.Queries.GetMe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserResponse>> Me(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _sender.Send(new GetMeQuery(userId), cancellationToken);
        if (!result.Found)
            return NotFound();

        return Ok(new UserResponse(
            result.Id!.Value,
            result.Email!,
            result.FirstName!,
            result.LastName!,
            result.State!.Value.ToString(),
            result.Roles,
            result.Permissions));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _sender.Send(
            new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword),
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ErrorCode == "IncorrectPassword")
                return BadRequest(new { error = result.ErrorCode });

            return NotFound();
        }

        DeleteRefreshCookie();
        return NoContent();
    }

    [EnableRateLimiting(RateLimitPolicies.Login)]
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

    [EnableRateLimiting(RateLimitPolicies.AccountEmail)]
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification(
        [FromBody] ResendVerificationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ResendVerificationCommand(request.Email), cancellationToken);
        return Accepted();
    }

    [EnableRateLimiting(RateLimitPolicies.AccountEmail)]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ForgotPasswordCommand(request.Email), cancellationToken);
        return Accepted();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResetPasswordCommand(request.Token, request.NewPassword),
            cancellationToken);
        return NoContent();
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
