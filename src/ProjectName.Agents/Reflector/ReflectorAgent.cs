using Application.Interfaces;
using PMCR.Core.Agents;
using PMCR.Core.Laws;
using PMCR.Core.Trails;

namespace Reflector;

public sealed class ReflectorAgent(IUnitOfWork uow, MafPhaseRunner maf)
{
    public async Task RunAsync(Guid trailId, CancellationToken ct = default)
    {
        var trail = await uow.GetRepository<ITrailRepository>().GetWithFramesAsync(trailId)
            ?? throw new InvalidOperationException($"Trail {trailId} not found.");
        EC_SYS_003.Enforce(trail.Frames.Any());
        var checker = trail.Frames.LastOrDefault(f => f.Role == "checker")
            ?? throw new InvalidOperationException("Reflector requires a Checker frame.");
        EC_004.EnforceCheckerVerdict("checker", checker.Verdict);
        var response = MafPhaseRunner.RequireText("reflector", await maf.RunAsync("reflector",
            "You are the PMCR-O Reflector. Analyze the completed cycle and identify durable lessons. Never override the Checker verdict.",
            $"Checker verdict: {checker.Verdict}\nChecker evidence: {checker.Evidence}\nIntent: {trail.Intent}\nReturn concise reflection and whether the evidence supports promotion.", ct));
        var status = checker.Verdict.Equals("PASS", StringComparison.OrdinalIgnoreCase) ? "ACCEPT" : "HALT";
        var evidence = $"MAF reflector response: {response}";
        EC_VERIFY_FIRST_001.Enforce(evidence);
        var envelope = System.Text.Json.JsonSerializer.Serialize(new { phase = "reflect", disposition = status, reflection = response });
        PLAN_001.Enforce(envelope);
        trail.Status = status.ToLowerInvariant();
        await uow.Repository<Domain.Entities.Trail>().UpdateAsync(trail);
        await uow.Repository<Domain.Entities.Frame>().AddAsync(new Domain.Entities.Frame
        {
            TrailId = trailId, Role = "reflector", Verdict = status, TypedEnvelopeJson = envelope, Evidence = evidence
        }, ct);
        await uow.SaveChangesAsync(ct);
    }
}
