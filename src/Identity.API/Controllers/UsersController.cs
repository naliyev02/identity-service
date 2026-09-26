using Identity.API.Contracts.Requests;
using Identity.API.Contracts.Responses;
using Identity.Application.Features.Users.Commands.Register;
using Identity.Application.Features.Users.Queries.GetUserById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;
    public UsersController(ISender sender) => _sender = sender;

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
        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(
    Guid id,
    CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserByIdQuery(id), cancellationToken);
        if (!result.Found)
            return NotFound();
        return Ok(new UserResponse(
            result.Id!.Value,
            result.Email!,
            result.FirstName!,
            result.LastName!,
            result.State!.Value.ToString()));
    }
}
