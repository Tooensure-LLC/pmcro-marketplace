// src/ProjectName.MakerGrpc/Services/MakerGrpcService.cs
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PMCR.Core.Agents;

namespace ProjectName.MakerGrpc;

/// <summary>
/// Stateless MAF facade mirroring Maker.MakerAgent's prompt, without the
/// trail persistence (uow/Frame) - that stays with the PMCR.Agents worker.
/// </summary>
public sealed class MakerGrpcService(
    ILogger<MakerGrpcService> logger,
    MafPhaseRunner maf) : Maker.MakerBase
{
    public override async Task<Response> RunCycle(Request request, ServerCallContext context)
    {
        logger.LogInformation("[gRPC] Maker.RunCycle: {Intent}", request.Intent);

        try
        {
            var prompt = $"Approved plan:\n{request.ApprovedPlan}\n\nIntent: {request.Intent}\nExecute the plan using available governed capabilities. Report concrete actions and evidence; do not issue PASS/LOOP/HALT.";
            var response = MafPhaseRunner.RequireText("maker", await maf.RunAsync(
                "maker",
                "You are the PMCR-O Maker. Execute the planner's bounded plan. You may report work and evidence, but you never score or approve your own work.",
                prompt,
                context.CancellationToken));
            return new Response { Result = response };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[gRPC] Maker.RunCycle failed");
            throw;
        }
    }
}
