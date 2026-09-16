# PMCRO Tool Reference

The same idea as Claude Code's own [Tools Reference](https://code.claude.com/docs/en/tools-reference):
one entry per real capability a PMCR-O agent can call in this repo, with an honest status —
**Implemented**, **Declared but unused**, or **Stub** — rather than describing the intended design as
if it were already true. Read every entry's source file before relying on it; this document
describes code that existed on 2026-09-16 and will drift.

## Trail primitives (`PMCR.Core.Trails.TrailService`)

| Member | Signature | What it does | Status |
|---|---|---|---|
| `OpenFrameAsync` | `(string role, string intent, string sourceType = "pmcro") -> Task<string>` | Creates `.pmcro/trails/{sourceType}/{uuid}/00-{role}.jsonl` on the **local filesystem** and writes one JSON line (`phase:"open"`, role, intent, timestamp, `law:"EC-SYS-003"`). Returns the trail directory path. | **Implemented** |
| `WriteFrameAsync` | `(string role, string trailPath, object data, string evidence = "") -> Task` | Appends one JSON line to `{trailPath-dir}/{role}.jsonl`. | **Implemented** |

**Real inconsistency, not yet reconciled:** `AppHost/Program.cs`'s own comment says *"Trails are not a
mounted folder any more: they live in Postgres, which is the whole point of this wiring."* `TrailService`
still writes to the local filesystem, and `OrchestratorAgent.StartCycleAsync` calls **both**
`TrailService.OpenFrameAsync` (filesystem) **and** `IUnitOfWork.Repository<Trail>().AddAsync` (Postgres,
via `Domain.Entities.Trail`) for the same cycle. There is currently no single source of truth for where
a trail actually lives.

## Persistence (`Application.Interfaces`, `Infrastructure.Persistence`)

| Member | What it does | Status |
|---|---|---|
| `IUnitOfWork.Repository<T>()` | Closed generic factory for any `T : BaseEntity` — this is why adding a new entity never means adding a new UoW property. | **Implemented** |
| `IUnitOfWork.GetRepository<TRepository>()` | Resolves a named custom repository (e.g. `ITrailRepository`) for querying beyond generic CRUD. | **Implemented** |
| `IGenericRepository<T>.{GetByIdAsync,GetAllAsync,FindAsync,AddAsync,UpdateAsync,DeleteAsync,CountAsync,Query}` | Standard CRUD + LINQ query surface over EF Core, with a soft-delete filter (`IsDeleted`) applied everywhere. | **Implemented** |
| `ITrailRepository.GetWithFramesAsync` / `GetOpenTrailsAsync` | Trail-specific queries (eager-load frames; list trails not yet sealed). | **Implemented** |

**Real gap corroborated in two places independently** (this file and `extern/pmcro-skills/.pmcro/laws.md`'s
`LAW-010`): `Domain.Common.AppendOnlyEntity` exists specifically so a trail frame can be made
un-updatable and un-deletable at the type level, but its own doc comment says *"nothing uses it yet"* —
`Frame : BaseEntity`, and `BaseEntity`-backed `IGenericRepository<T>` hands every entity, `Frame`
included, a working `UpdateAsync`/`DeleteAsync`. **A frame can currently be mutated or soft-deleted
through the generic repository, with nothing in code stopping it.** The fix is already scoped in
`AppendOnlyEntity.cs`'s own remarks: move `Frame` onto it, add an `IAppendOnlyRepository<T>` with no
update/delete methods, and `REVOKE UPDATE, DELETE ON frames` at the database layer. Treat that as its
own PMCR-O cycle, not a drive-by fix.

## Laws (`PMCR.Core.Laws`)

| Member | What it does | Status |
|---|---|---|
| `EC_SYS_003.Enforce(bool frameExists)` | Throws if no trail frame exists before a mutation. | **Declared but unused** — no `RunAsync` in any of the five `*Agent` classes calls this. |
| `EC_VERIFY_FIRST_001.Enforce(string evidence)` / `.IsValid(string typedEnvelopeJson)` | Rejects empty evidence or a literal `<placeholder>`/`TODO`. | **Declared but unused** |
| `EC_004.EnforceMakerDoesNotScore` / `.EnforceCheckerVerdict` | Throws if a Maker emits a verdict, or if a Checker emits anything outside `PASS\|LOOP\|HALT`. | **Declared but unused** |
| `EC_009.Enforce(int currentLoop, int maxLoops = 3)` / `.ShouldHalt` | Throws (or reports) once loop count reaches the cap. | **Declared but unused** |
| `PLAN_001.Enforce(string jsonl)` | Rejects an empty or `<placeholder>`-containing frame. Not present in `pmcro-skills`' law set at all — a sixth law unique to this repo. | **Declared but unused** |
| `EarnedConstraints.{NoHardcodedPaths,NoNewInUoW,TrailSealedImmutable}` | Three hardcoded string constants. Not the JSON-lifecycle model in `pmcro-runtime`, nor the markdown list in `extern/pmcro-skills/.pmcro/earned-constraints.md` — **a third, distinct Earned Constraints implementation.** Not yet reconciled with either. | **Declared but unused** |

This is the same finding the mined `AgentSkills.Template` reference made about a *different* repo:
*"your Checker gate is currently advice, not enforcement."* Here it's literal — every law is a real,
callable, throwing guard clause, and every phase agent's comment says "EC-SYS-003 enforced" directly
above a method body that never calls `EC_SYS_003.Enforce(...)`.

## OMode (`PMCR.Core.OMode.OModeSelector`)

| Member | What it does | Status |
|---|---|---|
| `Select(string intent, int loopCount)` | `loopCount >= 2` → `Patch`; intent contains `"colony"` → `Colony`; `intent.Length < 80` → `MetaFast`; else `Standard`. | **Implemented** — real, if simple, heuristic. Called once, in `OrchestratorAgent.StartCycleAsync`. |

## Phase agents (`PMCR.OrchestratorService`, `PMCR.Agents.*`)

| Member | What it does | Status |
|---|---|---|
| `OrchestratorAgent.StartCycleAsync(string intent)` | Selects an OMode, opens a filesystem trail frame, creates a `Trail` row in Postgres, saves. Returns the new trail's `Guid`. | **Implemented**, modulo the filesystem/Postgres inconsistency above. |
| `PlannerAgent.RunAsync` / `MakerAgent.RunAsync` / `CheckerAgent.RunAsync` / `ReflectorAgent.RunAsync` | Loads the trail with its frames, then — literally — `// ... do Planner work via MCP + CodeAct`, then adds a bare `Frame` row (`Role` set, everything else default: `Verdict = "PENDING"`, `TypedEnvelopeJson = "{}"`). | **Stub.** The comment names the intended mechanism (MCP tool calls + CodeAct); none of it is implemented. A `CheckerAgent.RunAsync` run today always produces a `Frame` with verdict `"PENDING"`, never `PASS`/`LOOP`/`HALT`. |

## Model runtime (`PMCR.OllamaRuntime`)

| Member | What it does | Status |
|---|---|---|
| `AddOllamaChatClient(this IHostApplicationBuilder, connectionName="llama4", model=null)` | Registers `OllamaApiClient` (OllamaSharp) as the app's `IChatClient`, resolving the Ollama endpoint from Aspire's injected connection string. Pinned default model `llama4:maverick` — the file's own comment explains why: *"a floating tag makes a trail unreplayable."* | **Implemented** |

## MCP servers (`dotagent.toml`)

| Server | Command | Status |
|---|---|---|
| `github` | `npx @modelcontextprotocol/server-github` | **Declared, not yet observed wired into any `Execute*` call in code** — no actuator class exists yet in `src/`. |
| `postgres` | `npx @modelcontextprotocol/server-postgres`, env from `ConnectionStrings__pmcro-db` | Same — declared only. |
| `filesystem` | `npx @modelcontextprotocol/server-filesystem /data` | Same — declared only. |

## Reading this table

"Declared but unused" and "Stub" are not criticisms to fix reflexively — they're an accurate map of
what a PMCR-O cycle run today would actually do (open a trail, pick an OMode, write near-empty
`PENDING` frames) versus what the laws and agent names promise. Wire one law into one agent, in one
real PMCR-O cycle with a real trail, rather than wiring all of them at once — the same discipline
`extern/pmcro-skills` already applies to itself.
