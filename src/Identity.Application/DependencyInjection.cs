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
        services.AddScoped<PasswordResetIssuer>();
        services.AddScoped<SessionIssuer>();
        services.AddScoped<CurrentSessionLocator>();
        services.AddOptions<AppOptions>().BindConfiguration(AppOptions.SectionName);
        services.AddOptions<JwtOptions>().BindConfiguration(JwtOptions.SectionName);
        services.AddOptions<LockoutOptions>().BindConfiguration(LockoutOptions.SectionName);
        return services;
    }
}
