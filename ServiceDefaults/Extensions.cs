using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public static class ServiceDefaultsExtensions
{
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();
        builder.Services.ConfigureHttpClientDefaults(http => http.AddStandardResilienceHandler());
        return builder;
    }
    public static IHostApplicationBuilder AddDefaultHealthChecks(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        return builder;
    }
}
