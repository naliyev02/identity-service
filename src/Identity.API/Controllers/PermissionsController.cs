using Identity.API.Contracts.Responses;
using Identity.Application.Authorization;
using Identity.Application.Features.Permissions.Queries.GetPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Route("api/permissions")]
[ApiController]
public class PermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public PermissionsController(ISender sender) => _sender = sender;

    [Authorize(Policy = AccessPolicies.RolesRead)]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetPermissionsQuery(), cancellationToken);
        return Ok(result.Permissions.Select(permission =>
            new PermissionResponse(permission.Name, permission.Description)));
    }
}
