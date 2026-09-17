using Polly;
using ProjectName.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- Aspire Defaults ---
builder.AddServiceDefaults();

// --- Ollama IChatClient ("model-orchestrator") for trail replay ---
// Reads connection string "ollama-server" + "Ollama:Models:Orchestrator".
builder.AddOllamaClients();

// --- gRPC clients for each PMCR-O phase facade ---
// Addresses are the AppHost resource names (see src/ProjectName.AppHost/AppHost.cs);
// http.AddServiceDiscovery() from AddServiceDefaults() resolves these to the real
// endpoint Aspire assigns at run time. Each facade is a stateless single-shot MAF
// call (see each *GrpcService.cs) — calling one of these is NOT the same as running
// a full Plan->Make->Check->Reflect cycle; ProjectName.OrchestratorGrpc.Run is only
// intent-routing triage, not a chain of the other four.
// Cold-Ollama-load timeout headroom lives in ProjectName.ServiceDefaults/Extensions.cs
// (AddServiceDefaults -> ConfigureHttpClientDefaults -> AddStandardResilienceHandler),
// applied process-wide, rather than per gRPC client here: an explicit per-client
// Configure<HttpStandardResilienceOptions>(clientName, ...) override attempted here
// first and was verified (live 500 response, same "00:00:30" timeout) to NOT take
// effect against ConfigureHttpClientDefaults's own named-options registration.
// PMCR-O cycles are long-running (Ollama inference, now with an extra
// load_skill round trip since UseToolApproval auto-approves skill tools —
// see PMCR.Core.Agents.MafPhaseRunner). The global ConfigureHttpClientDefaults
// resilience handler (AttemptTimeout 100s / TotalRequestTimeout 120s, see
// ServiceDefaults/Extensions.cs) is sized for a single cold-Ollama-load
// round trip, not two. Ported from archive/PMCR.OrchestratorService/Program.cs,
// which hit and fixed this exact class of timeout: strip the global resilience
// handler per gRPC client and replace it with a long timeout instead.

var plannerClient = builder.Services.AddGrpcClient<ProjectName.PlannerGrpc.Planner.PlannerClient>(o =>
    o.Address = new Uri("http://projectname-plannergrpc"));
plannerClient.RemoveAllResilienceHandlers();
plannerClient.AddResilienceHandler("pmcro-long-running", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromMinutes(10));
});
plannerClient.AddServiceDiscovery();

var makerClient = builder.Services.AddGrpcClient<ProjectName.MakerGrpc.Maker.MakerClient>(o =>
    o.Address = new Uri("http://projectname-makergrpc"));
makerClient.RemoveAllResilienceHandlers();
makerClient.AddResilienceHandler("pmcro-long-running", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromMinutes(10));
});
makerClient.AddServiceDiscovery();

var checkerClient = builder.Services.AddGrpcClient<ProjectName.CheckerGrpc.Checker.CheckerClient>(o =>
    o.Address = new Uri("http://projectname-checkergrpc"));
checkerClient.RemoveAllResilienceHandlers();
checkerClient.AddResilienceHandler("pmcro-long-running", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromMinutes(10));
});
checkerClient.AddServiceDiscovery();

var reflectorClient = builder.Services.AddGrpcClient<ProjectName.ReflectorGrpc.Reflector.ReflectorClient>(o =>
    o.Address = new Uri("http://projectname-reflectorgrpc"));
reflectorClient.RemoveAllResilienceHandlers();
reflectorClient.AddResilienceHandler("pmcro-long-running", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromMinutes(10));
});
reflectorClient.AddServiceDiscovery();

var orchestratorClient = builder.Services.AddGrpcClient<ProjectName.OrchestratorGrpc.Orchestrator.OrchestratorClient>(o =>
    o.Address = new Uri("http://projectname-orchestratorgrpc"));
orchestratorClient.RemoveAllResilienceHandlers();
orchestratorClient.AddResilienceHandler("pmcro-long-running", pipeline =>
{
    pipeline.AddTimeout(TimeSpan.FromMinutes(10));
});
orchestratorClient.AddServiceDiscovery();

// --- Controllers & OpenAPI ---
// Only discover controllers in THIS assembly, ignore referenced projects
builder.Services.AddControllers()
    .AddApplicationPart(typeof(Program).Assembly);
builder.Services.AddEndpointsApiExplorer();

// Generates the OpenAPI spec that Scalar will consume
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "PMCR-O Colony API";
        document.Info.Version = "v5.1";
        document.Info.Description = "Thin HTTP facade over OrchestratorService (gRPC). Provides synchronous and asynchronous cognitive loops.";
        return Task.CompletedTask;
    });
});

// --- CORS ---
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// --- Middleware Pipeline ---
app.UseCors();
app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    // Generate the /openapi/v1.json spec
    app.MapOpenApi();

    // Serve Scalar UI at the root path (/)
    app.MapScalarApiReference("/", options =>
    {
        options.WithTitle("PMCR-O API Reference")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Map the [ApiController] routes
app.MapControllers();

// Health check endpoint
app.MapGet("/healthz", () => "ProjectName.OrchestratorApi -- HTTP facade for OrchestratorService");

app.Run();