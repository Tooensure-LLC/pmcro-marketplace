// AppHost — Aspire composition root.
//
// Ported from the archive's AppHost, keeping the settings that were paid for with
// debugging and dropping the project wiring for services that do not exist here yet
// (the three MCP actuators, the Next.js frontend). Each of those comes back when its
// project does, not before - an AddProject for a project that is not in the solution
// does not compile, and a commented-out one rots.
//
// OrchestratorService/PlannerService were renamed to OrchestratorGrpc/PlannerGrpc;
// the wiring below points at the projects that actually exist in the solution.

var builder = DistributedApplication.CreateBuilder(args);

// ── Parameters ────────────────────────────────────────────────────────────────
// Values live in configuration, not in code, so a developer machine, a container,
// and CI can differ without an edit. .pmcro says what is governed; these say where.
var repoRoot = builder.AddParameter("repoRoot");
var workspaceRoot = builder.AddParameter("workspace-root");

// ── Ollama persistent GPU container ───────────────────────────────────────────
var ollama = builder
    .AddOllama("ollama-server")
    .WithGPUSupport(OllamaGpuVendor.Nvidia)
    // Persistent so model weights survive a restart; a fresh pull per run makes the
    // inner loop unusable once the model is measured in gigabytes.
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("ollama-data")
    // 16384 rather than the 4096 default: a full skill manifest plus progressive
    // disclosure does not fit in 4096, and the failure is silent truncation.
    .WithEnvironment("OLLAMA_CONTEXT_LENGTH", "16384")
    // Flash attention off. The archive records repeated SIGSEGV in
    // ggml_backend_sched_reserve during model load on an RTX 4070 Laptop GPU at this
    // context length with flash attention on. That mitigation was never confirmed
    // fixed by a sealed trail, so it is carried forward as-is: if crashes return, the
    // next lever is lowering OLLAMA_CONTEXT_LENGTH, not re-enabling this.
    .WithEnvironment("OLLAMA_FLASH_ATTENTION", "0");

var modelOrchestrator = ollama.AddModel("model-orchestrator", "qwen3:8b");

// ── gRPC facades ───────────────────────────────────────────────────────────
// Each one now calls AddOllamaClients() + AddPmcroMafRunner() in its own
// Program.cs (ProjectName.ServiceDefaults), so each also needs the same
// container reference OrchestratorApi already had - a service that resolves
// a keyed IChatClient but never got WithReference(ollama) fails at startup
// with no connection string, not at the point it tries to call Ollama.
var plannerGrpc = builder.AddProject<Projects.ProjectName_PlannerGrpc>("projectname-plannergrpc")
    .WithReference(ollama)
    .WaitFor(ollama);

var orchestratorGrpc = builder.AddProject<Projects.ProjectName_OrchestratorGrpc>("projectname-orchestratorgrpc")
    .WithReference(ollama)
    .WaitFor(ollama);

var makerGrpc = builder.AddProject<Projects.ProjectName_MakerGrpc>("projectname-makergrpc")
    .WithReference(ollama)
    .WaitFor(ollama);

var checkerGrpc = builder.AddProject<Projects.ProjectName_CheckerGrpc>("projectname-checkergrpc")
    .WithReference(ollama)
    .WaitFor(ollama);

var reflectorGrpc = builder.AddProject<Projects.ProjectName_ReflectorGrpc>("projectname-reflectorgrpc")
    .WithReference(ollama)
    .WaitFor(ollama);

// OrchestratorApi is an HTTP facade in front of all five gRPC phase services
// (see its Program.cs AddGrpcClient calls, each addressed as "http://<resource-name>").
// Each of those hostnames only resolves via Aspire service discovery if the resource
// is referenced here — a live run without these references produced a captured
// "No such host is known. (projectname-orchestratorgrpc:80)" RpcException even
// though orchestratorgrpc itself was up and Running.
// Captured (not fire-and-forget like the OrchestratorApi registration used to
// be) because the MAUI app below needs this resource for WithReference - the
// same "projectname-orchestratorapi" name a MAUI HttpClient's
// "https+http://projectname-orchestratorapi" BaseAddress resolves via service
// discovery, once ProjectName.App itself calls AddServiceDefaults().
var orchestratorApi = builder.AddProject<Projects.ProjectName_OrchestratorApi>("projectname-orchestratorapi")
    .WithReference(ollama)
    .WithReference(plannerGrpc)
    .WithReference(makerGrpc)
    .WithReference(checkerGrpc)
    .WithReference(reflectorGrpc)
    .WithReference(orchestratorGrpc)
    .WaitFor(ollama)
    .WaitFor(plannerGrpc)
    .WaitFor(makerGrpc)
    .WaitFor(checkerGrpc)
    .WaitFor(reflectorGrpc)
    .WaitFor(orchestratorGrpc);

// ── ProjectName.Agents workers ────────────────────────────────────────────
// The trail-persisting MAF cycles. Previously not AppHost resources at all,
// so Aspire never started them and they never received a connection string
// for the MafPhaseRunner their constructors require.
// NOTE: these also call AddInfrastructure() for Postgres-backed trail
// persistence; this AppHost does not yet define a Postgres resource despite
// referencing Aspire.Hosting.PostgreSQL, so a connection string still needs
// to come from configuration (or a builder.AddPostgres(...) added here)
// before these will run end-to-end. That gap is pre-existing and separate
// from the Ollama/MAF wiring done in this pass.
builder.AddProject<Projects.ProjectName_Agents_Planner>("pmcro-planner")
    .WithReference(ollama)
    .WaitFor(ollama);

builder.AddProject<Projects.ProjectName_Agents_Maker>("pmcro-maker")
    .WithReference(ollama)
    .WaitFor(ollama);

builder.AddProject<Projects.ProjectName_Agents_Checker>("pmcro-checker")
    .WithReference(ollama)
    .WaitFor(ollama);

builder.AddProject<Projects.ProjectName_Agents_Reflector>("pmcro-reflector")
    .WithReference(ollama)
    .WaitFor(ollama);

// ── ProjectName.App (MAUI marketplace client) ─────────────────────────────
// Added via Aspire.Hosting.Maui, not AddProject<T> - ProjectName.App
// multi-targets net11.0-android/-windows10.0.19041.0, incompatible TFMs for
// an AddProject<T> reference against this net11.0 AppHost, and the docs
// explicitly say not to add a ProjectReference for the same reason. The path
// below is resolved by AddMauiProject itself (MSBuild invoked out-of-process
// per platform head), so it is intentionally a string, not Projects.ProjectName_App.
//
// Only Windows and Android are wired up: ProjectName.App.csproj's
// TargetFrameworks list is net11.0-android plus a Windows-conditional
// net11.0-windows10.0.19041.0 and nothing for ios/maccatalyst, so
// AddiOSSimulator()/AddMacCatalystDevice() would reference platform heads
// that don't exist in the project and fail at build time, not startup.
var mauiApp = builder.AddMauiProject("projectname-app", "../../ProjectName.App/ProjectName.App.csproj");

// Windows runs directly on the host machine and can reach localhost, so no
// dev tunnel is needed for the OrchestratorApi reference here.
mauiApp.AddWindowsDevice()
    .WithReference(orchestratorApi)
    .WaitFor(orchestratorApi);

// Android emulator cannot reach the host's localhost, so both the API traffic
// and the OTLP telemetry need a dev tunnel. WithOtlpDevTunnel() creates and
// wires its own tunnel for telemetry; androidApiTunnel is the separate one
// WithReference(orchestratorApi, androidApiTunnel) below routes HTTP through,
// per the two-tunnel pattern in the Aspire MAUI docs (one resource, two
// tunnels: OTLP and API are configured independently).
var androidApiTunnel = builder.AddDevTunnel("projectname-app-devtunnel")
    .WithAnonymousAccess()
    .WithReference(orchestratorApi.GetEndpoint("https"));

mauiApp.AddAndroidEmulator()
    .WithOtlpDevTunnel()
    .WithReference(orchestratorApi, androidApiTunnel)
    .WaitFor(orchestratorApi);

builder.Build().Run();