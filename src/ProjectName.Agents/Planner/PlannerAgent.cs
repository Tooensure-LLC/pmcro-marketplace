using Application.Interfaces;
using PMCR.Core.Agents;
using PMCR.Core.Laws;
using PMCR.Core.Trails;

namespace Planner;

public sealed class PlannerAgent(IUnitOfWork uow, MafPhaseRunner maf)
{
    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId)
            ?? throw new InvalidOperationException($"Trail {trailId} not found.");
        EC_SYS_003.Enforce(trail.Frames.Any());
        var prompt = $"Intent: {trail.Intent}\nReturn a minimum executable PMCR-O plan. Include ordered actions, resources, and verification criteria.";
        var response = MafPhaseRunner.RequireText("planner", await maf.RunAsync("planner",
            "You are the PMCR-O Planner. Produce only a minimal, concrete execution plan. Never claim execution or verification.", prompt, ct));
        var evidence = $"MAF planner response: {response}";
        EC_VERIFY_FIRST_001.Enforce(evidence);
        var envelope = System.Text.Json.JsonSerializer.Serialize(new { phase = "plan", status = "ready", plan = response });
        PLAN_001.Enforce(envelope);
        await uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "planner", Verdict = "READY", TypedEnvelopeJson = envelope, Evidence = evidence
        }, ct);
        await uow.SaveChangesAsync(ct);
    }
}
