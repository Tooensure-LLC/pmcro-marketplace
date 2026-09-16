// Aspire 13.5.3 AppHost. Every API here exists in that version.
//
// WHAT CHANGED FROM THE PREVIOUS FILE
//   builder.AddVolume(...)          - not an Aspire API. Removed.
//   .WithVolumeMount(trails, path)  - not an Aspire API. Removed.
// Trails are not a mounted folder any more: they live in Postgres, which is the
// whole point of this wiring. Persistence for the DB is WithDataVolume().

var builder = DistributedApplication.CreateBuilder(args);

// ---- Postgres: where the trail lives -------------------------------------
// A persistent volume requires a stable credential. Resolve it from the
// machine's user-scoped Parameters__postgres_password configuration value.
var postgresPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: postgresPassword)
                      .WithDataVolume("pmcro-pgdata-v3")
                      .WithPgAdmin();

var trailsDb = postgres.AddDatabase("trails");

// ---- Ollama: local inference ---------------------------------------------
// From CommunityToolkit.Aspire.Hosting.Ollama, not core Aspire.
// The model tag is pinned. A floating tag makes a trail unreplayable, because
// nobody can say afterwards which weights produced a frame.
var ollama = builder.AddOllama("ollama")
                    .WithDataVolume();

var model = ollama.AddModel("llama4", "llama4:maverick");

// ---- The Orchestrator: sole tool authority -------------------------------
// WaitFor is not decoration. The Orchestrator opens a trail frame before it
// does anything else (EC-SYS-003), so it must not start before the database
// that frame is written to - otherwise Log Before Act fails on a cold start,
// which is exactly when nobody is watching.
var orchestrator = builder.AddProject<Projects.PMCR_OrchestratorService>("orchestrator")
                          .WithHttpEndpoint(port: 5100, name: "http")
                          .WithHttpHealthCheck("/health")
                          .WithReference(trailsDb, "pmcro-db").WaitFor(trailsDb)
                          .WithReference(model);

// Phase agents execute inside the Orchestrator process. This keeps one tool
// authority and one trail writer instead of creating five idle service copies.

builder.Build().Run();
