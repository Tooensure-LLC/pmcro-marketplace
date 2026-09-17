using Application.Interfaces;
using PMCR.Core.Agents;
using PMCR.Core.Laws;
using PMCR.Core.Trails;

namespace Maker;

public sealed class MakerAgent(IUnitOfWork uow, MafPhaseRunner maf)
{
    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId)
            ?? throw new InvalidOperationException($"Trail {trailId} not found.");
        EC_SYS_003.Enforce(trail.Frames.Any());
        var plan = trail.Frames.LastOrDefault(f => f.Role == "planner")
            ?? throw new InvalidOperationException("Maker requires a planner frame.");
        var prompt = $"Approved plan:\n{plan.Evidence}\n\nIntent: {trail.Intent}\nExecute the plan using available governed capabilities. Report concrete actions and evidence; do not issue PASS/LOOP/HALT.";
        var response = MafPhaseRunner.RequireText("maker", await maf.RunAsync("maker",
            "You are the PMCR-O Maker. Execute the planner's bounded plan. You may report work and evidence, but you never score or approve your own work.", prompt, ct));
        var evidence = $"MAF maker response: {response}";
        EC_VERIFY_FIRST_001.Enforce(evidence);
        var envelope = System.Text.Json.JsonSerializer.Serialize(new { phase = "make", status = "done", result = response });
        PLAN_001.Enforce(envelope);
        EC_004.EnforceMakerDoesNotScore("maker", string.Empty);
        await uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "maker", Verdict = string.Empty, TypedEnvelopeJson = envelope, Evidence = evidence
        }, ct);
        await uow.SaveChangesAsync(ct);
    }
}
