using Identity.API.Contracts.Responses;
using Identity.API.Cookies;
using Identity.API.Extensions;
using Identity.Application.Authorization;
using Identity.Application.Features.Authentication.Commands.RevokeMySession;
using Identity.Application.Features.Authentication.Commands.RevokeOtherSessions;
using Identity.Application.Features.Authentication.Queries.GetMySessions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Route("api/auth/sessions")]
[ApiController]
public class SessionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _environment;

    public SessionsController(ISender sender, IWebHostEnvironment environment)
    {
        _sender = sender;
        _environment = environment;
    }

    [Authorize(Policy = AccessPolicies.SessionsRead)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SessionResponse>>> List(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _sender.Send(
            new GetMySessionsQuery(userId, ReadRefreshToken()),
            cancellationToken);

        return Ok(result.Sessions.Select(session => new SessionResponse(
            session.Id,
            session.CreatedAtUtc,
            session.ExpiresAtUtc,
            session.IsCurrent)));
    }

    [Authorize(Policy = AccessPolicies.SessionsRevoke)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _sender.Send(
            new RevokeMySessionCommand(userId, id, ReadRefreshToken()),
            cancellationToken);

        if (!result.Found)
            return NotFound();

        if (result.WasCurrent)
            DeleteRefreshCookie();

        return NoContent();
    }

    [Authorize(Policy = AccessPolicies.SessionsRevoke)]
    [HttpDelete]
    public async Task<IActionResult> RevokeOthers(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
            return Unauthorized();

        await _sender.Send(
            new RevokeOtherSessionsCommand(userId, ReadRefreshToken()),
            cancellationToken);

        return NoContent();
    }

    private string? ReadRefreshToken()
        => Request.Cookies.TryGetValue(RefreshTokenCookie.Name, out var refreshToken)
            ? refreshToken
            : null;

    private void DeleteRefreshCookie()
        => Response.Cookies.Delete(
            RefreshTokenCookie.Name,
            RefreshTokenCookie.Create(_environment.IsDevelopment()));
}
