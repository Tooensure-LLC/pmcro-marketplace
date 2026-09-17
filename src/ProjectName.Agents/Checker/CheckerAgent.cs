using Application.Interfaces;
using PMCR.Core.Agents;
using PMCR.Core.Laws;
using PMCR.Core.Trails;

namespace Checker;

public sealed class CheckerAgent(IUnitOfWork uow, MafPhaseRunner maf)
{
    public async Task<string> RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId)
            ?? throw new InvalidOperationException($"Trail {trailId} not found.");
        EC_SYS_003.Enforce(trail.Frames.Any());
        var priorLoops = trail.Frames.Count(f => f.Role == "checker" && f.Verdict.Equals("LOOP", StringComparison.OrdinalIgnoreCase));
        if (EC_009.ShouldHalt(priorLoops)) { await AddVerdict(trailId, "HALT", "Maximum Checker loop count reached.", ct); return "HALT"; }
        var evidence = string.Join("\n\n", trail.Frames.Select(f => $"[{f.Role}] {f.Evidence}"));
        var prompt = $"Intent: {trail.Intent}\nEvidence:\n{evidence}\n\nReturn exactly one verdict token first: PASS, LOOP, or HALT. Then explain the evidence. PASS only when the requested outcome is demonstrably complete.";
        var response = MafPhaseRunner.RequireText("checker", await maf.RunAsync("checker",
            "You are the PMCR-O Checker and the sole phase allowed to issue PASS, LOOP, or HALT. Verify evidence, do not perform work.", prompt, ct));
        var token = response.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToUpperInvariant() ?? "HALT";
        var verdict = token is "PASS" or "LOOP" or "HALT" ? token : "HALT";
        EC_VERIFY_FIRST_001.Enforce(response);
        EC_004.EnforceCheckerVerdict("checker", verdict);
        await AddVerdict(trailId, verdict, $"MAF checker response: {response}", ct);

        return verdict;
    }

    private async Task AddVerdict(Guid trailId, string verdict, string evidence, CancellationToken ct)
    {
        var envelope = System.Text.Json.JsonSerializer.Serialize(new { phase = "check", verdict, evidence });
        PLAN_001.Enforce(envelope);
        await uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame { TrailId = trailId, Role = "checker", Verdict = verdict, TypedEnvelopeJson = envelope, Evidence = evidence }, ct);
        await uow.SaveChangesAsync(ct);
    }
}
