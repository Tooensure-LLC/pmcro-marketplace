// src/ProjectName.ServiceDefaults/MafExtensions.cs
//
// Single shared home for MAF wiring. Every PMCR-O host (the PMCR.Agents workers
// and the *Grpc/*Api facades) already references ServiceDefaults, so the
// MafPhaseRunner is registered here once instead of each host re-implementing
// its own copy — one module, referenced everywhere, not nine near-duplicates.
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PMCR.Core.Agents;

namespace ProjectName.ServiceDefaults;

public static class MafExtensions
{
    /// <summary>
    /// Default location the MarketplaceSkillsMaterializer mirrors registered skills into
    /// (see ARCH-MAF-NATIVE-002). Relative to the host's content root.
    /// </summary>
    public const string DefaultSkillsStagingPath = ".pmcro/skills-staging";

    /// <summary>
    /// Registers a singleton <see cref="MafPhaseRunner"/> bound to the
    /// "model-orchestrator" keyed <see cref="IChatClient"/> registered by
    /// <see cref="OllamaExtensions.AddOllamaClients{TBuilder}"/>. Call
    /// <c>AddOllamaClients()</c> first — this does not add it for you, so a
    /// missing call fails fast with a clear DI error rather than silently
    /// resolving a client no one configured.
    /// </summary>
    public static TBuilder AddPmcroMafRunner<TBuilder>(this TBuilder builder, string? skillsPath = null)
        where TBuilder : IHostApplicationBuilder
    {
        IConfiguration config = builder.Configuration;
        var resolvedSkillsPath = skillsPath
            ?? config["Skills:StagingPath"]
            ?? DefaultSkillsStagingPath;
        var fullPath = Path.IsPathRooted(resolvedSkillsPath)
            ? resolvedSkillsPath
            : Path.Combine(AppContext.BaseDirectory, resolvedSkillsPath);

        // Mirror plugin skills into fullPath now, at builder configuration time,
        // so they're on disk before MafPhaseRunner (and the AgentSkillsProvider
        // it constructs) is ever resolved. See MarketplaceSkillsMaterializer.
        MarketplaceSkillsMaterializer.Materialize(fullPath);

        builder.Services.AddSingleton(sp =>
        {
            var chatClient = sp.GetRequiredKeyedService<IChatClient>(OllamaExtensions.Keys.Orchestrator);
            return new MafPhaseRunner(chatClient, fullPath);
        });

        return builder;
    }
}
