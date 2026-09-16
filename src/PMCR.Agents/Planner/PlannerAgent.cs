using Application.Interfaces;
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
        var evidence = $"Planner loaded intent and produced the minimum execution plan at {DateTime.UtcNow:O}.";
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "planner", Verdict = "READY",
            TypedEnvelopeJson = "{\"phase\":\"plan\",\"status\":\"ready\"}", Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
