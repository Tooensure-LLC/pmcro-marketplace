// src/ProjectName.ReflectorGrpc/Services/ReflectorGrpcService.cs
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PMCR.Core.Agents;
using PMCR.Core.Laws;

namespace ProjectName.ReflectorGrpc;

/// <summary>
/// Stateless MAF facade mirroring Reflector.ReflectorAgent's prompt, without
/// the trail persistence (uow/Frame/Trail.Status) - that stays with the
/// PMCR.Agents worker.
/// </summary>
public sealed class ReflectorGrpcService(
    ILogger<ReflectorGrpcService> logger,
    MafPhaseRunner maf) : Reflector.ReflectorBase
{
    public override async Task<Response> RunCycle(Request request, ServerCallContext context)
    {
        logger.LogInformation("[gRPC] Reflector.RunCycle: {Intent}", request.Intent);

        try
        {
            EC_004.EnforceCheckerVerdict("checker", request.CheckerVerdict);

            var prompt = $"Checker verdict: {request.CheckerVerdict}\nChecker evidence: {request.CheckerEvidence}\nIntent: {request.Intent}\nReturn concise reflection and whether the evidence supports promotion.";
            var response = MafPhaseRunner.RequireText("reflector", await maf.RunAsync(
                "reflector",
                "You are the PMCR-O Reflector. Analyze the completed cycle and identify durable lessons. Never override the Checker verdict.",
                prompt,
                context.CancellationToken));

            var disposition = request.CheckerVerdict.Equals("PASS", StringComparison.OrdinalIgnoreCase) ? "ACCEPT" : "HALT";
            return new Response { Disposition = disposition, Reflection = response };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[gRPC] Reflector.RunCycle failed");
            throw;
        }
    }
}
