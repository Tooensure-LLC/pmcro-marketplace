using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace PMCR.OllamaRuntime;

/// <summary>
/// Binds a local Ollama model as the colony's <see cref="IChatClient"/>.
/// </summary>
/// <remarks>
/// REPLACES the previous contents of this file, which did:
/// <code>new OllamaChatClient(uri, model).AsIChatClient()</code>
/// There is no <c>OllamaChatClient</c> type to construct and nothing to call
/// <c>.AsIChatClient()</c> on. OllamaSharp's <c>OllamaApiClient</c> implements
/// <see cref="IChatClient"/> itself.
///
/// A provider is a substitution, never an exception. Swapping a hosted model for
/// a local one changes latency, cost and quality. It changes no law: Log Before
/// Act still holds, the Maker still cannot self-score, and the Orchestrator is
/// still the only thing that executes a tool.
/// </remarks>
public static class OllamaRuntimeExtensions
{
    /// <summary>Default model tag. Pinned - a floating tag makes a trail unreplayable.</summary>
    public const string DefaultModel = "llama4:maverick";

    public static IHostApplicationBuilder AddOllamaChatClient(
        this IHostApplicationBuilder builder,
        string connectionName = "llama4",
        string? model = null)
    {
        // Endpoint comes from configuration, injected by Aspire's WithReference.
        // A literal http://ollama:11434 binds the colony to one deployment
        // topology and fails the moment anyone runs it anywhere else.
        var endpoint = builder.Configuration.GetConnectionString(connectionName)
            ?? throw new InvalidOperationException(
                $"No connection string '{connectionName}'. The AppHost supplies it via " +
                $"WithReference(model); running this service standalone needs it in " +
                $"configuration.");

        builder.Services.AddChatClient(_ =>
            new OllamaApiClient(new Uri(endpoint), model ?? DefaultModel));

        return builder;
    }
}
