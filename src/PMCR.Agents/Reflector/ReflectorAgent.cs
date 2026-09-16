using Application.Interfaces;
using PMCR.Core.Trails;

namespace PMCR.Agents.Reflector;

public sealed class ReflectorAgent
{
    private readonly IUnitOfWork _uow;
    private readonly TrailService _trails;
    public ReflectorAgent(IUnitOfWork uow, TrailService trails) { _uow = uow; _trails = trails; }

    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await _uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId);
        if (trail is null) throw new InvalidOperationException($"Trail {trailId} not found.");
        var checker = trail.Frames.LastOrDefault(f => f.Role == "checker");
        if (checker is null) throw new InvalidOperationException("Reflector requires a Checker frame.");
        var status = checker.Verdict == "PASS" ? "ACCEPT" : "HALT";
        var evidence = checker.Verdict == "PASS"
            ? "Checker PASS observed; Reflector sealed the cycle as ACCEPT."
            : "Checker did not PASS; Reflector halted promotion.";
        trail.Status = status.ToLowerInvariant();
        await _uow.Repository<Domain.Entities.Trail>().UpdateAsync(trail);
        await _uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "reflector", Verdict = status,
            TypedEnvelopeJson = $"{{\"phase\":\"reflect\",\"disposition\":\"{status}\"}}", Evidence = evidence
        }, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
