using Application.Interfaces;
using PMCR.Core.OMode;
using PMCR.Core.Trails;

namespace PMCR.OrchestratorService;

public class OrchestratorAgent
{
    private readonly IUnitOfWork _uow;
    private readonly TrailService _trails;
    private readonly OModeSelector _oMode;
    private readonly PMCR.Agents.Planner.PlannerAgent _planner;
    private readonly PMCR.Agents.Maker.MakerAgent _maker;
    private readonly PMCR.Agents.Checker.CheckerAgent _checker;
    private readonly PMCR.Agents.Reflector.ReflectorAgent _reflector;

    public OrchestratorAgent(
        IUnitOfWork uow,
        TrailService trails,
        OModeSelector oMode,
        PMCR.Agents.Planner.PlannerAgent planner,
        PMCR.Agents.Maker.MakerAgent maker,
        PMCR.Agents.Checker.CheckerAgent checker,
        PMCR.Agents.Reflector.ReflectorAgent reflector)
    {
        _uow = uow; _trails = trails; _oMode = oMode;
        _planner = planner; _maker = maker; _checker = checker; _reflector = reflector;
    }

    public async Task<Guid> StartCycleAsync(string intent, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(intent)) throw new ArgumentException("Intent is required.", nameof(intent));
        var mode = _oMode.Select(intent, 0);
        var trailPath = await _trails.OpenFrameAsync("orchestrator", intent, mode.ToString());
        var trail = new Domain.Entities.Trail { Intent = intent, SourceType = mode.ToString() };
        await _uow.Repository<Domain.Entities.Trail>().AddAsync(trail, ct);
        await _uow.SaveChangesAsync(ct);
        await _planner.RunAsync(trail.Id, ct);
        await _maker.RunAsync(trail.Id, ct);
        await _checker.RunAsync(trail.Id, ct);
        await _reflector.RunAsync(trail.Id, ct);
        return trail.Id;
    }
}
