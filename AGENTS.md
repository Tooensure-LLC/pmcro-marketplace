# AGENTS.md — Root Contract

All agents MUST:
1. EC-SYS-003 Log Before Act — open .pmcro/trails/{type}/{uuid}/{NN}-{role}.jsonl BEFORE any file mutation.
2. EC-VERIFY-FIRST-001 — Maker must cite real evidence (command output, diffs).
3. EC-004 — Maker never self-scores. Only Checker issues PASS/LOOP/HALT.
4. EC-009 — MaxLoops=3, halt to human.
5. PLAN-001 — every frame is schema-valid JSONL, no `<placeholder>` left.

All five exist as real, callable guard clauses in `src/PMCR.Core/Laws/` — but as of 2026-09-16 none
of them are actually called by any phase agent. See `.pmcro/tool-reference.md` for the full,
honest status of every real capability in this repo (what's implemented, declared-but-unused, or a
stub) before assuming any of the above is enforced rather than just true in principle.

PMCR-O role prompts (the actual instruction source `dotagent.toml` loads) live at
`plugins/pmcro/skills/{orchestrator,planner,maker,checker,reflector}/SKILL.md` in this repo —
PMCR-O roles here are C# services guided by those skill files, not skill packages executed
directly. `extern/pmcro-skills/` is governance/reference content (the laws' original markdown form,
MAF-alignment notes, declarative templates) folded in from a sibling repo — read it for context and
conventions, not as the live prompt source.

## Repository layout (verify against live disk before trusting this — it drifts)

- `src/api/` — the unified backend. Every REST/gRPC service that the Aspire AppHost provisions
  lives here as a sibling project: `ProjectName.CheckerGrpc`, `ProjectName.MakerGrpc`,
  `ProjectName.ReflectorGrpc`, `ProjectName.OrchestratorApi` (REST/Minimal API surface),
  `ProjectName.OrchestratorGrpc`, `ProjectName.PlannerGrpc`. gRPC-to-gRPC (agent/service) traffic
  stays inside these projects' Protos; REST is Minimal APIs in `OrchestratorApi` only — do not mix
  the two transports inside one project.
- `src/PMCR.Agents/{Checker,Maker,Planner,Reflector}/` — the MAF agent *logic* (AIAgent/workflow
  code), referenced by the matching `*Grpc`/`*Api` host project in `src/api/`. This is a separate
  layer from the gRPC service hosts and was NOT folded into `src/api/` — only the host/service
  projects were consolidated there.
- `src/ProjectName.AppHost/` — the real, solution-wired Aspire AppHost (`pmcro.slnx` and
  `AppHost.csproj`'s `ProjectReference`s are the source of truth for what it provisions). It
  references every project in `src/api/`, Ollama, and Redis/Postgres.
- `src/ProjectName.ServiceDefaults/` — the real, solution-wired shared Aspire defaults project,
  referenced by every `src/api/` project.
- `archive/` — stale/superseded, not part of the build: a duplicate root-level `AppHost/` and
  `ServiceDefaults/` (orphaned copies never wired into `pmcro.slnx`), and
  `PMCR.OrchestratorService` (earlier MAF orchestrator logic, superseded by
  `ProjectName.OrchestratorApi` + `ProjectName.OrchestratorGrpc`). Do not resurrect these by editing
  in place; if real logic is needed from them, port it deliberately into the live `src/api/`
  project.
- No cloud AI endpoints anywhere in `src/api/` — Ollama via `Microsoft.Extensions.AI` /
  `OllamaSharp` only. `Microsoft.Agents.AI*` (MAF) packages, not Semantic Kernel or AutoGen.
