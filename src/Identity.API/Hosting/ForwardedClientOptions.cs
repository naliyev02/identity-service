namespace Identity.API.Hosting;

public sealed class ForwardedClientOptions
{
    public const string SectionName = "ForwardedHeaders";

    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];
    public int ForwardLimit { get; set; } = 1;
}
