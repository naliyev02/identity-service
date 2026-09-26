using Identity.Domain.User.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Middleware;

public sealed class ExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            IdentityDomainException ex => (
                StatusCodes.Status400BadRequest,
                "Domain rule violated",
                ex.Message),
            Npgsql.NpgsqlException => (
                StatusCodes.Status503ServiceUnavailable,
                "Database unavailable",
                "Could not connect to the database."),
            Microsoft.EntityFrameworkCore.DbUpdateException => (
                StatusCodes.Status400BadRequest,
                "Persistence error",
                "Could not save changes."),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Server error",
                "An unexpected error occurred.")
        };
        context.Response.StatusCode = status;
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };
        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
