using System.ComponentModel;
using System.Text.Json;
using Microsoft.Agents.AI;
using ModelContextProtocol.Server;
using Pmcro.McpServer.Models;

namespace Pmcro.McpServer.Tools;

[McpServerToolType]
public sealed class PmcroTools(GovernedPhaseAgentFactory agents)
{
    [McpServerTool(Name = "pmcro_plan")]
    [Description("Planner phase: refine seed intent into a minimum executable PMCR-O plan.")]
    public async Task<IntentEnvelope> PlanAsync(
        [Description("Initial intent or goal.")] string seedIntent,
        CancellationToken ct)
    {
        var cycleId = Guid.NewGuid().ToString("D");
        var response = await RunAsync("planner",
            "Produce a minimum executable PMCR-O plan. Do not claim execution or verification.",
            $"Seed intent: {seedIntent}", ct);
        return Envelope(cycleId, seedIntent, response, "plan");
    }

    [McpServerTool(Name = "pmcro_make")]
    [Description("Maker phase: execute a supplied plan only when Checker verification is present.")]
    public async Task<IntentEnvelope> MakeAsync(
        [Description("PMCR-O cycle identifier.")] string cycleId,
        [Description("Plan to execute.")] string plan,
        [Description("Checker verification must be true before mutation-oriented execution.")] bool verified,
        CancellationToken ct)
    {
        if (!verified)
            throw new InvalidOperationException("EC-VERIFY-FIRST-001: pmcro_make requires verified=true.");
        var response = await RunAsync("maker",
            "Execute only the supplied plan. Do not self-score. Cite concrete evidence and artifacts.",
            $"Cycle: {cycleId}\nVerified: {verified}\nPlan: {plan}", ct);
        return Envelope(cycleId, plan, response, "make");
    }

    [McpServerTool(Name = "pmcro_check")]
    [Description("Checker phase: verify the cycle and return PASS, LOOP, or HALT evidence.")]
    public async Task<IntentEnvelope> CheckAsync(
        [Description("PMCR-O cycle identifier.")] string cycleId,
        [Description("Artifact or execution evidence to check.")] string evidence,
        CancellationToken ct)
    {
        var response = await RunAsync("checker",
            "Verify the supplied evidence against the stated cycle. Return exactly one verdict: PASS, LOOP, or HALT, with reasons. Never perform mutations.",
            $"Cycle: {cycleId}\nEvidence: {evidence}", ct);
        return Envelope(cycleId, evidence, response, "check");
    }

    [McpServerTool(Name = "pmcro_reflect")]
    [Description("Reflector phase: analyze a completed check and identify durable learning or a law candidate.")]
    public async Task<IntentEnvelope> ReflectAsync(
        [Description("PMCR-O cycle identifier.")] string cycleId,
        [Description("Checker outcome and supporting evidence.")] string outcome,
        CancellationToken ct)
    {
        var response = await RunAsync("reflector",
            "Reflect on the completed cycle. Identify durable learning and a candidate earned constraint. Do not rewrite history or claim acceptance.",
            $"Cycle: {cycleId}\nOutcome: {outcome}", ct);
        return Envelope(cycleId, outcome, response, "reflect");
    }

    [McpServerTool(Name = "pmcro_orchestrate")]
    [Description("Orchestrator phase: turn a seed intent into a governed PMCR-O cycle proposal with loop limits.")]
    public async Task<IntentEnvelope> OrchestrateAsync(
        [Description("Initial intent or goal.")] string seedIntent,
        [Description("Current loop count; EC-009 halts at three loops.")] int loopCount,
        CancellationToken ct)
    {
        if (loopCount >= 3)
            throw new InvalidOperationException("EC-009: MaxLoops=3 reached; halt to human.");
        var response = await RunAsync("orchestrator",
            "Coordinate the PMCR-O cognitive phases without bypassing Planner, Maker, Checker, or Reflector. Return the next governed phase sequence and acceptance criteria.",
            $"Seed intent: {seedIntent}\nCurrent loop count: {loopCount}", ct);
        return Envelope(Guid.NewGuid().ToString("D"), seedIntent, response, "orchestrate");
    }

    private async Task<string> RunAsync(string role, string instructions, string input, CancellationToken ct)
    {
        AIAgent agent = agents.Create(role, instructions);
        var response = await agent.RunAsync(input, cancellationToken: ct);
        if (string.IsNullOrWhiteSpace(response.Text))
            throw new InvalidOperationException($"MAF agent '{role}' returned no response.");
        return response.Text.Trim();
    }

    private static IntentEnvelope Envelope(string cycleId, string seed, string response, string phase) => new()
    {
        CycleId = cycleId,
        SeedIntent = seed,
        TruestIntent = response,
        Phase = phase,
        Evidence = ["MAF response returned through governed phase agent."],
        Steps = [new IntentStep { Id = 1, Action = phase, Target = "PMCRO governed phase" }]
    };
}
