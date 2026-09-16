using Application.Interfaces;
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
        var ready = trail.Frames.Any(f => f.Role == "planner") && trail.Frames.Any(f => f.Role == "maker");
        var verdict = ready ? "PASS" : "LOOP";
        var evidence = ready ? "Planner and Maker evidence present; Checker issued PASS." : "Required phase evidence missing; Checker issued LOOP.";
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "checker", Verdict = verdict,
            TypedEnvelopeJson = $"{{\"phase\":\"check\",\"verdict\":\"{verdict}\"}}", Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
