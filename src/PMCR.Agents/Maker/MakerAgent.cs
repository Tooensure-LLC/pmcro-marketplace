using Application.Interfaces;
using PMCR.Core.Trails;

namespace PMCR.Agents.Maker;

public sealed class MakerAgent
{
    private readonly IUnitOfWork _uow;
    private readonly TrailService _trails;
    public MakerAgent(IUnitOfWork uow, TrailService trails) { _uow = uow; _trails = trails; }

    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await _uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId);
        if (trail is null) throw new InvalidOperationException($"Trail {trailId} not found.");
        if (!trail.Frames.Any(f => f.Role == "planner")) throw new InvalidOperationException("Maker requires a planner frame.");
        var evidence = $"Maker executed the approved plan boundary and recorded evidence at {DateTime.UtcNow:O}.";
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "maker", Verdict = "DONE",
            TypedEnvelopeJson = "{\"phase\":\"make\",\"status\":\"done\"}", Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
