using Identity.API.Contracts.Requests;
using Identity.API.Contracts.Responses;
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

    public AuthController(ISender sender) => _sender = sender;

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
}
