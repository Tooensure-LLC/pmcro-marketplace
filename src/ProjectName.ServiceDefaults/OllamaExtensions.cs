// src/ProjectName.ServiceDefaults/OllamaExtensions.cs
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration; // 👈 CRITICAL: Resolves CS1061
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace ProjectName.ServiceDefaults;

public static class OllamaExtensions
{
    public static class Keys
    {
        public const string Orchestrator = "model-orchestrator";
    }

    public static TBuilder AddOllamaClients<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        // builder.Configuration is an IConfigurationManager. 
        // We use it as IConfiguration to access extension methods.
        IConfiguration config = builder.Configuration;

        builder.Services.AddKeyedSingleton<IChatClient>(Keys.Orchestrator, (sp, _) =>
        {
            var raw = config.GetConnectionString("ollama-server")
                  ?? "http://localhost:11434";

            var endpoint = ParseEndpoint(raw);

            var model = config["Ollama:Models:Orchestrator"] ?? "qwen3:8b";

            var http = new HttpClient { BaseAddress = endpoint, Timeout = Timeout.InfiniteTimeSpan };
            return new OllamaApiClient(http) { SelectedModel = model };
        });

        return builder;
    }

    /// <summary>
    /// Aspire/CommunityToolkit container connection strings are not always a bare
    /// URL — they can arrive as "Key=Value;Key2=Value2" (e.g. "Endpoint=http://
    /// localhost:11434"), which throws UriFormatException ("The URI scheme is not
    /// valid") if passed straight to <see cref="Uri"/>. This accepts either form.
    /// </summary>
    private static Uri ParseEndpoint(string raw)
    {
        if (Uri.TryCreate(raw, UriKind.Absolute, out var direct))
        {
            return direct;
        }

        foreach (var part in raw.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 &&
                kv[0].Trim().Equals("Endpoint", StringComparison.OrdinalIgnoreCase) &&
                Uri.TryCreate(kv[1].Trim(), UriKind.Absolute, out var fromKv))
            {
                return fromKv;
            }
        }

        throw new UriFormatException(
            $"Could not parse an endpoint URI from Ollama connection string: \"{raw}\"");
    }
}