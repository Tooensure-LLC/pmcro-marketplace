// src/ProjectName.PlannerGrpc/Services/PlannerGrpcService.cs
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PMCR.Core.Agents;

namespace ProjectName.PlannerGrpc;

/// <summary>
/// Stateless MAF facade mirroring Planner.PlannerAgent's prompt, without the
/// trail persistence (uow/Frame) - that stays with the PMCR.Agents worker.
/// </summary>
public sealed class PlannerGrpcService(
    ILogger<PlannerGrpcService> logger,
    MafPhaseRunner maf) : Planner.PlannerBase
{
    public override async Task<Response> RunCycle(Request request, ServerCallContext context)
    {
        logger.LogInformation("[gRPC] Planner.RunCycle: {Intent}", request.MessySeedIntent);

        try
        {
            var prompt = $"Intent: {request.MessySeedIntent}\nReturn a minimum executable PMCR-O plan. Include ordered actions, resources, and verification criteria.";
            var response = MafPhaseRunner.RequireText("planner", await maf.RunAsync(
                "planner",
                "You are the PMCR-O Planner. Produce only a minimal, concrete execution plan. Never claim execution or verification.",
                prompt,
                context.CancellationToken));
            return new Response { TrueSeedIntent = response };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[gRPC] Planner.RunCycle failed");
            throw;
        }
    }
}
