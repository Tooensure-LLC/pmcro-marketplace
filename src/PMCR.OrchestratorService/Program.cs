using Application.Interfaces;
using PMCR.Agents.Checker;
using PMCR.Agents.Maker;
using PMCR.Agents.Planner;
using PMCR.Agents.Reflector;
using PMCR.Core.OMode;
using PMCR.Core.Trails;
using PMCR.OllamaRuntime;
using PMCR.OrchestratorService;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddInfrastructure(builder.Configuration);
builder.AddOllamaChatClient();
builder.Services.AddSingleton<TrailService>();
builder.Services.AddSingleton<OModeSelector>();
builder.Services.AddScoped<PlannerAgent>();
builder.Services.AddScoped<MakerAgent>();
builder.Services.AddScoped<CheckerAgent>();
builder.Services.AddScoped<ReflectorAgent>();
builder.Services.AddScoped<OrchestratorAgent>();

var app = builder.Build();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/cycles", async (CreateCycleRequest request, OrchestratorAgent orchestrator, CancellationToken ct) =>
{
    if (string.IsNullOrWhiteSpace(request.Intent)) return Results.BadRequest(new { error = "Intent is required." });
    var id = await orchestrator.StartCycleAsync(request.Intent.Trim(), ct);
    return Results.Accepted($"/api/cycles/{id}", new { id, status = "accepted" });
});

app.MapGet("/api/cycles/{id:guid}", async (Guid id, IUnitOfWork uow, CancellationToken ct) =>
{
    var trail = await uow.GetRepository<ITrailRepository>().GetWithFramesAsync(id);
    if (trail is null) return Results.NotFound();
    return Results.Ok(new
    {
        id = trail.Id,
        cycleId = trail.CycleId,
        trail.Intent,
        trail.SourceType,
        trail.Status,
        trail.LoopCount,
        frames = trail.Frames.Select(f => new
        {
            f.Id, f.Role, f.CycleNumber, f.Verdict, f.TypedEnvelopeJson, f.Evidence
        })
    });
});

app.MapGet("/api/runtime", () => Results.Ok(new
{
    service = "pmcro-orchestrator",
    model = OllamaRuntimeExtensions.DefaultModel,
    phases = new[] { "orchestrator", "planner", "maker", "checker", "reflector" }
}));

app.Run();

public sealed record CreateCycleRequest(string Intent);
