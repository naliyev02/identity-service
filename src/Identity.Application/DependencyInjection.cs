using Identity.Application.Options;
using Identity.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddScoped<EmailVerificationIssuer>();
        services.AddOptions<AppOptions>().BindConfiguration(AppOptions.SectionName);
        return services;
    }
}
