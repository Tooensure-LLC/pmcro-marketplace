// src/ProjectName.CheckerGrpc/Services/CheckerGrpcService.cs
using Grpc.Core;
using Microsoft.Extensions.Logging;
using PMCR.Core.Agents;
using PMCR.Core.Laws;

namespace ProjectName.CheckerGrpc;

/// <summary>
/// Stateless MAF facade mirroring Checker.CheckerAgent's prompt, without the
/// trail persistence or loop-count enforcement (EC_009) - those require a
/// trail and stay with the PMCR.Agents worker.
/// </summary>
public sealed class CheckerGrpcService(
    ILogger<CheckerGrpcService> logger,
    MafPhaseRunner maf) : Checker.CheckerBase
{
    public override async Task<Response> RunCycle(Request request, ServerCallContext context)
    {
        logger.LogInformation("[gRPC] Checker.RunCycle: {Intent}", request.Intent);

        try
        {
            var prompt = $"Intent: {request.Intent}\nEvidence:\n{request.Evidence}\n\nReturn exactly one verdict token first: PASS, LOOP, or HALT. Then explain the evidence. PASS only when the requested outcome is demonstrably complete.";
            var response = MafPhaseRunner.RequireText("checker", await maf.RunAsync(
                "checker",
                "You are the PMCR-O Checker and the sole phase allowed to issue PASS, LOOP, or HALT. Verify evidence, do not perform work.",
                prompt,
                context.CancellationToken));

            var token = response.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToUpperInvariant() ?? "HALT";
            var verdict = token is "PASS" or "LOOP" or "HALT" ? token : "HALT";
            EC_004.EnforceCheckerVerdict("checker", verdict);

            return new Response { Verdict = verdict, Explanation = response };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[gRPC] Checker.RunCycle failed");
            throw;
        }
    }
}
