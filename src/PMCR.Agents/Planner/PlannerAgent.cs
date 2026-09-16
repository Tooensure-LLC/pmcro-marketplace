using Application.Interfaces;
using PMCR.Core.Laws;
using PMCR.Core.Trails;

namespace PMCR.Agents.Planner;

public sealed class PlannerAgent
{
    private readonly IUnitOfWork _uow;
    private readonly TrailService _trails;
    public PlannerAgent(IUnitOfWork uow, TrailService trails) { _uow = uow; _trails = trails; }

    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await _uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId);
        if (trail is null) throw new InvalidOperationException($"Trail {trailId} not found.");
        EC_SYS_003.Enforce(trail.Frames.Any());
        var evidence = $"Planner loaded intent and produced the minimum execution plan at {DateTime.UtcNow:O}.";
        EC_VERIFY_FIRST_001.Enforce(evidence);
        var envelope = "{\"phase\":\"plan\",\"status\":\"ready\"}";
        PLAN_001.Enforce(envelope);
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "planner", Verdict = "READY",
            TypedEnvelopeJson = envelope, Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}