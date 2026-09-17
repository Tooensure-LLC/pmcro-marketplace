// src/ProjectName.OrchestratorGrpc/Services/OrchestratorGrpcService.cs
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PMCR.Core.Agents;

namespace ProjectName.OrchestratorGrpc;

/// <summary>
/// Stateless MAF facade. This does not persist a trail (that is
/// PMCR.Agents' job) - it exists for synchronous, replayable single-shot
/// orchestration calls (e.g. trail replay from ProjectName.OrchestratorApi).
/// </summary>
public sealed class OrchestratorGrpcService(
    ILogger<OrchestratorGrpcService> logger,
    MafPhaseRunner maf) : Orchestrator.OrchestratorBase
{
    public override async Task<Response> Run(Request request, ServerCallContext context)
    {
        logger.LogInformation("[gRPC] Orchestrator.Run: {Intent}", request.MessySeedIntent);

        try
        {
            var response = MafPhaseRunner.RequireText("orchestrator", await maf.RunAsync(
                "orchestrator",
                "You are the PMCR-O Orchestrator. You route a messy seed intent to the " +
                "correct phase and law, and you are the only thing that executes a tool. " +
                "You never plan, make, check, or reflect yourself.",
                request.MessySeedIntent,
                context.CancellationToken));
            return new Response { TrueSeedIntent = response };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[gRPC] Orchestrator.Run failed");
            throw;
        }
    }
}
