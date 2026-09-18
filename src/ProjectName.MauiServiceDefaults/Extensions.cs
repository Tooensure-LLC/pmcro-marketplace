using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace ProjectName.MauiServiceDefaults
{
    // Client-side counterpart of ProjectName.ServiceDefaults.Extensions for MAUI
    // heads (Windows, Android). No AddAspNetCoreInstrumentation, no health-check
    // endpoints, no FrameworkReference to Microsoft.AspNetCore.App - a MAUI app
    // is an HTTP client of the Aspire-orchestrated services, never a server
    // itself.
    public static class Extensions
    {
        public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.ConfigureOpenTelemetry();

            builder.Services.AddServiceDiscovery();

            builder.Services.ConfigureHttpClientDefaults(http =>
            {
                // Same headroom as ProjectName.ServiceDefaults.Extensions:
                // any client call that fans out to Ollama through OrchestratorApi
                // can take longer than the 30s default TotalRequestTimeout.
                http.AddStandardResilienceHandler(options =>
                {
                    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(100);
                    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(200);
                    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(120);
                });

                http.AddServiceDiscovery();
            });

            return builder;
        }

        public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            builder.Logging.AddOpenTelemetry(logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

            builder.Services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddHttpClientInstrumentation()
                        .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(builder.Environment.ApplicationName)
                        .AddHttpClientInstrumentation();
                });

            builder.AddOpenTelemetryExporters();

            return builder;
        }

        private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
        {
            // Set by Aspire.Hosting.Maui's WithOtlpDevTunnel()/dev-tunnel wiring
            // (Windows: localhost directly; Android emulator: the OTLP dev tunnel
            // URL) - never hardcoded here, per the "no cloud AI hardcodes,
            // local-first" constraint.
            var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

            if (useOtlpExporter)
            {
                builder.Services.AddOpenTelemetry().UseOtlpExporter();
            }

            return builder;
        }
    }
}
