using System.Net;
using Identity.API.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

namespace Identity.API.Extensions;

public static class ForwardedHeadersExtensions
{
    public static IServiceCollection AddForwardedClientHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configured = configuration.GetSection(ForwardedClientOptions.SectionName).Get<ForwardedClientOptions>()
                         ?? new ForwardedClientOptions();
        if (configured.ForwardLimit <= 0)
            throw new InvalidOperationException("ForwardedHeaders:ForwardLimit must be positive.");

        var proxies = configured.KnownProxies.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();
        var networks = configured.KnownNetworks.Where(value => !string.IsNullOrWhiteSpace(value)).ToArray();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
            if (proxies.Length == 0 && networks.Length == 0)
            {
                options.ForwardedHeaders = ForwardedHeaders.None;
                return;
            }

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = configured.ForwardLimit;
            foreach (var proxy in proxies)
                options.KnownProxies.Add(ParseAddress(proxy));
            foreach (var network in networks)
                options.KnownNetworks.Add(ParseNetwork(network));
        });

        return services;
    }

    private static IPAddress ParseAddress(string value)
    {
        if (IPAddress.TryParse(value, out var address))
            return address;

        throw new InvalidOperationException($"ForwardedHeaders:KnownProxies contains an invalid address '{value}'.");
    }

    private static IPNetwork ParseNetwork(string value)
    {
        if (IPNetwork.TryParse(value, out var network))
            return network;

        throw new InvalidOperationException($"ForwardedHeaders:KnownNetworks contains an invalid network '{value}'.");
    }
}
