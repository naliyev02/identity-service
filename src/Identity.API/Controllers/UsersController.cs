using Identity.API.Contracts.Requests;
using Identity.API.Contracts.Responses;
using Identity.API.RateLimiting;
using Identity.Application.Authorization;
using Identity.Application.Features.Roles.Commands.AssignRolesToUser;
using Identity.Application.Features.Users.Commands.Register;
using Identity.Application.Features.Users.Commands.UpdateUserStatus;
using Identity.Application.Features.Users.Queries.GetUserById;
using Identity.Application.Features.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    public UsersController(ISender sender) => _sender = sender;

    [EnableRateLimiting(RateLimitPolicies.AccountEmail)]
    [HttpPost]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RegisterUserCommand(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName),
            cancellationToken);
        if (!result.Succeeded)
        {
            if (result.ErrorCode == "EmailAlreadyExists")
                return Conflict(new { error = result.ErrorCode });
            return BadRequest();
        }
        var response = new RegisterUserResponse(
            result.Id!.Value,
            result.Email!,
            result.FirstName!,
            result.LastName!,
            result.State!.Value.ToString());
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize(Policy = AccessPolicies.UsersRead)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result.Users.Select(user => new UserResponse(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.State,
            user.Roles,
            user.Permissions)));
    }

    [Authorize(Policy = AccessPolicies.UsersRead)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        if (!result.Found)
            return NotFound();

        return Ok(ToResponse(result));
    }

    [Authorize(Policy = AccessPolicies.UsersWrite)]
    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> AssignRoles(
        Guid id,
        [FromBody] AssignRolesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignRolesToUserCommand(id, request.RoleIds ?? []),
            cancellationToken);

        if (result.Succeeded)
            return NoContent();

        if (result.ErrorCode == "UserNotFound")
            return NotFound();

        return BadRequest(new { error = result.ErrorCode });
    }

    [Authorize(Policy = AccessPolicies.UsersWrite)]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateUserStatusCommand(id, request.Status),
            cancellationToken);

        if (result.Succeeded)
            return NoContent();

        if (result.ErrorCode == "UserNotFound")
            return NotFound();

        return BadRequest(new { error = result.ErrorCode });
    }

    private static UserResponse ToResponse(GetUserByIdResult result)
        => new(
            result.Id!.Value,
            result.Email!,
            result.FirstName!,
            result.LastName!,
            result.State!.Value.ToString(),
            result.Roles,
            result.Permissions);
}
