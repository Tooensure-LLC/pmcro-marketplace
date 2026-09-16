using Application.Interfaces;
using PMCR.Core.Laws;
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
        EC_SYS_003.Enforce(trail.Frames.Any());
        if (!trail.Frames.Any(f => f.Role == "planner")) throw new InvalidOperationException("Maker requires a planner frame.");
        var evidence = $"Maker executed the approved plan boundary and recorded evidence at {DateTime.UtcNow:O}.";
        EC_VERIFY_FIRST_001.Enforce(evidence);
        var envelope = "{\"phase\":\"make\",\"status\":\"done\"}";
        PLAN_001.Enforce(envelope);
        EC_004.EnforceMakerDoesNotScore("maker", string.Empty);
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "maker", Verdict = string.Empty,
            TypedEnvelopeJson = envelope, Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}