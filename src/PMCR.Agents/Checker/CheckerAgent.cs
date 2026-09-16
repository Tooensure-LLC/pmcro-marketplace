using Application.Interfaces;
using PMCR.Core.Laws;
using PMCR.Core.Trails;

namespace PMCR.Agents.Checker;

public sealed class CheckerAgent
{
    private readonly IUnitOfWork _uow;
    private readonly TrailService _trails;
    public CheckerAgent(IUnitOfWork uow, TrailService trails) { _uow = uow; _trails = trails; }

    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await _uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId);
        if (trail is null) throw new InvalidOperationException($"Trail {trailId} not found.");
        EC_SYS_003.Enforce(trail.Frames.Any());
        var ready = trail.Frames.Any(f => f.Role == "planner") && trail.Frames.Any(f => f.Role == "maker");
        var priorLoops = trail.Frames.Count(f => f.Role == "checker" && string.Equals(f.Verdict, "LOOP", StringComparison.OrdinalIgnoreCase));
        if (EC_009.ShouldHalt(priorLoops))
        {
            await AddVerdict(trailId, "HALT", "Maximum Checker loop count reached; cycle halted.", ct);
            return;
        }
        var verdict = ready ? "PASS" : "LOOP";
        var evidence = ready ? "Planner and Maker evidence present; Checker issued PASS." : "Required phase evidence missing; Checker issued LOOP.";
        EC_VERIFY_FIRST_001.Enforce(evidence);
        var envelope = $"{{\"phase\":\"check\",\"verdict\":\"{verdict}\"}}";
        PLAN_001.Enforce(envelope);
        EC_004.EnforceCheckerVerdict("checker", verdict);
        await AddVerdict(trailId, verdict, evidence, ct);
    }

    private async Task AddVerdict(Guid trailId, string verdict, string evidence, CancellationToken ct)
    {
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "checker", Verdict = verdict,
            TypedEnvelopeJson = $"{{\"phase\":\"check\",\"verdict\":\"{verdict}\"}}", Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}