using Identity.API.Contracts.Requests;
using Identity.API.Contracts.Responses;
using Identity.Application.Authorization;
using Identity.Application.Features.Roles.Commands.AssignPermissions;
using Identity.Application.Features.Roles.Commands.CreateRole;
using Identity.Application.Features.Roles.Queries.GetRoleById;
using Identity.Application.Features.Roles.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Route("api/roles")]
[ApiController]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender) => _sender = sender;

    [Authorize(Policy = AccessPolicies.RolesRead)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRolesQuery(), cancellationToken);
        return Ok(result.Roles.Select(role => new RoleResponse(role.Id, role.Name, role.Permissions)));
    }

    [Authorize(Policy = AccessPolicies.RolesRead)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetRoleByIdQuery(id), cancellationToken);
        if (!result.Found)
            return NotFound();

        return Ok(new RoleResponse(result.Id!.Value, result.Name!, result.Permissions));
    }

    [Authorize(Policy = AccessPolicies.RolesWrite)]
    [HttpPost]
    public async Task<ActionResult<RoleResponse>> Create(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateRoleCommand(request.Name), cancellationToken);
        if (!result.Succeeded)
        {
            if (result.ErrorCode == "RoleAlreadyExists")
                return Conflict(new { error = result.ErrorCode });

            return BadRequest(new { error = result.ErrorCode });
        }

        var response = new RoleResponse(result.Id!.Value, result.Name!, []);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [Authorize(Policy = AccessPolicies.RolesWrite)]
    [HttpPut("{id:guid}/permissions")]
    public async Task<IActionResult> AssignPermissions(
        Guid id,
        [FromBody] AssignPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new AssignPermissionsToRoleCommand(id, request.Permissions ?? []),
            cancellationToken);

        if (result.Succeeded)
            return NoContent();

        if (result.ErrorCode == "RoleNotFound")
            return NotFound();

        return BadRequest(new { error = result.ErrorCode });
    }
}
