using System.Globalization;
using System.Threading.RateLimiting;
using Identity.API.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Identity.API.Extensions;

public static class RateLimitExtensions
{
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
                     ?? new RateLimitOptions();
        EnsurePositive(limits);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteTooManyRequests;
            options.AddPolicy(RateLimitPolicies.Login, context =>
                FixedWindow(context, limits.LoginPermitLimit, limits.LoginWindowSeconds));
            options.AddPolicy(RateLimitPolicies.AccountEmail, context =>
                FixedWindow(context, limits.EmailPermitLimit, limits.EmailWindowSeconds));
        });

        return services;
    }

    private static RateLimitPartition<string> FixedWindow(HttpContext context, int permitLimit, int windowSeconds)
        => RateLimitPartition.GetFixedWindowLimiter(
            ClientAddress(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = permitLimit,
                Window = TimeSpan.FromSeconds(windowSeconds),
                QueueLimit = 0
            });

    private static string ClientAddress(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        if (address is null)
            return "unknown";

        if (address.IsIPv4MappedToIPv6)
            address = address.MapToIPv4();

        return address.ToString();
    }

    private static async ValueTask WriteTooManyRequests(OnRejectedContext context, CancellationToken cancellationToken)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            var seconds = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
            context.HttpContext.Response.Headers.RetryAfter = seconds;
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "TooManyRequests" }, cancellationToken);
    }

    private static void EnsurePositive(RateLimitOptions limits)
    {
        if (limits.LoginPermitLimit <= 0 || limits.LoginWindowSeconds <= 0
            || limits.EmailPermitLimit <= 0 || limits.EmailWindowSeconds <= 0)
            throw new InvalidOperationException("RateLimit permit and window values must be positive.");
    }
}
