---
imported: 2026-09-18
source: claude.ai project export, project "PMCRO" (uuid 01a038f0-a27f-7158-b826-09beb82e83be), doc CONTEXT.md
doc_dated: 2026-08-26
---

# pmcro-skills context — 2026-08-26 snapshot (external, unverified against live disk)

**Ground truth notice:** this describes the sibling `pmcro-skills` repo
(`https://github.com/PMCRO-AI-Agent-Company/pmcro-skills`, mounted at `T:\pmcro-skills` /
formerly `Z:\pmcro-skills`) as it was on 2026-08-26 — it is not a description of
`pmcro-marketplace`'s own implementation, and it has not been re-verified against live
disk since it was written. Treat every claim below as dated evidence, not current fact.

**Known to already be stale relative to this repo, as of this import:**
`pmcro-marketplace`'s own `docs/governance/` and its `extern/pmcro-skills/` mirror both
still reflect the *older* EC-law era (`identity.json`/`catalog.json`/EC-prefixed laws,
plain `.pmcro/laws.md`) that this document itself says was superseded on the live
`pmcro-skills` repo by commit `77a27af` (2026-08-25) — the L-prefixed law corpus and
`manifest.yaml`/`laws.yaml`/capabilities-registry architecture below. Reconciling that
gap is Cycle B's job (migrating `pmcro-skills` into `pmcro-marketplace`), not this
import's — this file is filed as evidence for that cycle, unmodified.

---

# Context — pmcro-skills

What this project is, not how to act in it (see `INSTRUCTIONS.md` for that).

Rewritten 2026-08-25 against a direct filesystem read of `Z:\pmcro-skills`
after discovering the prior version described a superseded runtime
(pre-`77a27af`, EC-law/`identity.json`/`catalog.json` system). If anything
here looks stale in a future session, re-read `.pmcro/manifest.yaml` and
`.claude-plugin/marketplace.json` live rather than trusting this file.

## What This Repo Is

`pmcro-skills` holds the PMCR-O governance kernel, orchestration,
strategy, trail, actuator, and MAF-integration plugins — the runtime
whose `.pmcro/manifest.yaml` you load to become the Orchestrator.

## Stack & Identity

- Company: Tooensure (`org.tooensure`), owner Shawn
- Runtime: `pmcro.ai/v1` `ColonyRuntime`, version `2026.08` (`.pmcro/manifest.yaml`)
- Architecture: PMCR-O, execution substrate = Microsoft Agent Framework (MAF),
  workflow mode = declarative-first
- MCP protocol: `2026-07-28`
- Primary LLM: Claude (Anthropic)
- Platform: Desktop Commander on Windows, mapped to `Z:\`
- Repo remote: `https://github.com/PMCRO-AI-Agent-Company/pmcro-skills` (`main`)

## Plugins (this repo — verified 2026-08-26 by listing `plugins/` directly)

Five plugins exist on disk, none of them named `pmcro-orchestrator`,
`pmcro-strategy`, `pmcro-trail`, `pmcro-actuator`, or `pmcro-maf` — that
framing described a runtime that was never built here and has been
removed from this file and all three marketplace mirrors:

- `pmcr-o` — core PMCRO orchestration plugin: ultra-minimalist governance
  kernel owning four durable capabilities (activate, orchestrate, govern,
  trail) and the five phase roles (orchestrator, planner, maker, checker,
  reflector); command operations and schemas live as assets inside the
  capability skills
- `copilotkit` — CopilotKit UI/application-layer contract for PMCRO:
  renders governance state (Laws, Constraints, Acceptability, Cycle,
  Gate, Evidence, Trail, Approval, Disposition) via AG-UI and requests
  approvals, without ever becoming an authority itself
- `github` — governed GitHub repository, pull request, Actions, MCP,
  security, release, issue, code-search, and Agent Skills operations
- `prompt-engineering` — framework-agnostic reasoning techniques
  (Chain-of-Thought, Tree-of-Thought, Graph-of-Thought, ReAct, Optimize,
  Options) as self-contained Skill Commands any role may call directly
- `skill-creator` — MAF-native Agent Skill authoring plugin; scaffolds
  distributable skills (`create-skill`) and, as an internal helper
  `create-skill` invokes when needed, plugin shells (`create-plugin`,
  not independently user-routable)

Read each plugin's own `plugin.json` for its exact `version` and
`keywords` rather than trusting this summary long-term.

## Governance Kernel (`.pmcro/laws/laws.yaml`)

Seven laws, plain `L-` ids (not the old `EC-`/`COMPANY-`/`MAAI-` corpus):

- `L-EVIDENCE` — completion requires evidence
- `L-CHECKER-GATE` — Checker must pass before completion
- `L-STATE-MEMORY` — workflow state is not shared memory
- `L-AGENT-MEMORY` — agent memory is scoped to agent
- `L-CAPABILITY` — agents use skills/tools for capabilities, never invent one
- `L-ORCHESTRATION` — orchestrator owns routing, not domain implementation
- `L-RESEARCH` — version-sensitive decisions require current authoritative evidence

## Lifecycle (`.pmcro/state/STATE.md`, `.pmcro/runtime/orchestrator/ORCHESTRATOR.md`)

```
INTENT -> FRAME -> TRAIL -> EXECUTE -> CHECK -> REFLECT -> EVIDENCE -> SEAL
```
Failure transitions to `RETRY`, `ESCALATE`, or `HALT`. A cycle cannot
execute without a Frame+Trail, and cannot SEAL without a passed Checker
gate and collected Evidence. The declarative workflow expressing this is
`.pmcro/workflows/declarative/pmcro-cycle.yaml` (`InvokeAgent` sequence:
orchestrator -> planner -> maker -> checker -> [reflector on pass /
GotoAction retry on fail], `maxIterations: 3`).

## Authority Split (`docs/maf-native-architecture.md`)

PMCRO is an extension of MAF, not a replacement for its agent/skill/tool/
Harness/CodeAct/approval/looping/observability primitives. MAF owns the
execution substrate; PMCRO owns governed execution semantics (laws,
constraints, acceptability, P/M/C/R role separation, trails/replay).
Docker MCP Toolkit owns MCP catalog/profile/server-lifecycle/gateway
infrastructure; PMCRO does not reimplement it (no `pmcro-github-mcp`,
`pmcro-docker-mcp`, `pmcro-playwright-mcp`, or `pmcro-secret-mcp` — none
exist in this repo and none should be built unless a genuinely
PMCRO-specific capability emerges that Docker's own tooling can't cover).

## Capability / Provider / MCP Layer

Abstract capabilities (`.pmcro/capabilities/registry.yaml`) are stable
PMCRO contracts, independent of which provider implements them today:
`browser`, `containers`, `evidence`, `filesystem`, `github`,
`host-command-execution`, `lifecycle`, `mcp-gateway`, `memory`.

Providers (`.pmcro/providers/registry.yaml`) implement capabilities:
`github` (official GitHub MCP), `playwright` (official Playwright MCP),
`docker-mcp-toolkit` (gateway; owns catalog/profile/server lifecycle for
`containers`), `terminal` (native tool, `host-command-execution`,
**disabled by default, approval required, not a substitute for Docker
MCP Toolkit**), `maf-native` (filesystem), `pmcro-runtime` (memory,
evidence, evaluation — PMCRO-owned).

MCP routing (`.pmcro/mcp/registry.yaml`, `.pmcro/mcp/docker-toolkit.yaml`):
`github->github`, `browser->playwright`, `mcp-gateway`/`containers`
`->docker-mcp-toolkit`, `host-command-execution->terminal`. Docker MCP
Toolkit is the source of truth for actual profile/catalog state; the
`.pmcro` files express PMCRO's desired contract and guardrails only.

Docker MCP profiles (`.pmcro/config/profiles.yaml`) — **corrected
2026-08-26, verified live against Docker MCP Toolkit**: exactly one
profile exists, `pmcro`, containing one server (`playwright`). There is
no `github` server and no `pmcro-dev`/`pmcro-test`/`pmcro-prod` split;
`environments.yaml` independently confirms Development/Test/Production
all currently point at the same shared `pmcro` profile/catalog — no
real environment isolation exists yet. Gated by `policies/execution.yaml`.

Environments (`.pmcro/config/environments.yaml`): `Development`, `Test`,
`Production`, each mapping to a Docker MCP profile/catalog, a secret
source (`dotnet-user-secrets-or-docker-mcp-secret` / `ci-secret-store` /
`platform-secret-provider`), an approval mode, and `terminal:
disabled-by-default`.

Secrets flow: Aspire (app config/secrets) -> PMCRO runtime -> Docker MCP
Gateway -> MCP provider. `.pmcro/secrets/` and `.pmcro/config/
parameters.yaml` hold references and policy only, never values.

## Policies (`.pmcro/policies/`)

`execution.yaml` (cross-references `permissions.yaml` and `network.yaml`
via `policyReferences`), `security.yaml`, `permissions.yaml` (role
authority matrix — e.g. maker `may: [execute-approved-action,
emit-execution-evidence]`, `mayNot: [approve-own-action, verify-outcome,
seal-cycle]`; "No role may approve its own mutation."), `network.yaml`
(`default: deny`; browser/container network rules).

## Memory — path inconsistency resolved 2026-08-26

`.pmcro/manifest.yaml` and `.pmcro/runtime/config.yaml` declare
`agentMemory: agent-memory/<role>/` (top-level, sibling of `memory/`).
`.pmcro/memory/MEMORY.md` previously disagreed, describing
`memory/agent/<role>/`. Resolved by checking the consuming runtime
(`Z:\pmcro-runtime`) directly rather than cross-referencing docs within
this catalog repo: `.pmcro/agent-memory/seed-queue.json` exists and is
referenced by real sealed trail frames and test code; no
`.pmcro/memory/agent/` directory exists anywhere. `MEMORY.md` has been
corrected to `agent-memory/<role>/` to match. Treat any future doc that
still says `memory/agent/<role>/` as stale.

Shared Colony memory: `.pmcro/memory/` — validated, cross-cycle knowledge.
Promotion gate: `cycle -> evidence -> validation -> memory candidate ->
approval -> memory`. A failed/unverified cycle must never become
authoritative memory.

## Trails, Frames, Evidence, State, Evaluation

- `.pmcro/trails/` — `active -> completed|failed -> archived`.
  **Corrected 2026-08-26 by direct listing**: 8 sealed trails under
  `completed/`, 1 under `failed/` (`a1b2c3d4...`, a Checker-caught
  ungated write to `queue/pending/`, sealed REJECT rather than deleted),
  and a live queue (`queue/pending/`, `queue/done/`) with real seed
  intents. Still don't assume a prior session's narration is accurate —
  check the actual frame files — but "empty" is no longer true as of
  this correction.
- `.pmcro/frames/` — normalized intent; immutable once execution begins.
- `.pmcro/evidence/` — `executions/`, `checks/`, `artifacts/`, `provenance/`.
  "A completion claim without evidence is not a valid PMCR-O completion."
- `.pmcro/state/` — `schemas/`, `runs/`, `checkpoints/`, `locks/`;
  ephemeral/durable execution context, not knowledge memory.
- `.pmcro/evaluation/` — recommended suites only (Planner trail validity,
  Maker artifact correctness, Checker precision/recall, Reflector
  actionability, Orchestrator routing correctness, evidence completeness,
  workflow recovery/checkpoint behavior); no eval infra committed yet.

## Workspace Layout

```
Z:/pmcro-skills/                         <- workspace root (this repo)
  .pmcro/manifest.yaml                   <- root ColonyRuntime manifest
  .pmcro/laws/laws.yaml                  <- L- law corpus (7 laws)
  .pmcro/runtime/config.yaml             <- RuntimeConfig (maxIterations, gates)
  .pmcro/runtime/orchestrator/ORCHESTRATOR.md
  .pmcro/state/STATE.md                  <- cycle-state contract
  .pmcro/memory/MEMORY.md                <- shared/agent memory boundary
  .pmcro/capabilities/                   <- registry.yaml + per-capability contracts
  .pmcro/providers/                      <- registry.yaml + per-provider contracts
  .pmcro/mcp/                            <- registry.yaml, docker-toolkit.yaml
  .pmcro/config/                         <- parameters.yaml, environments.yaml, profiles.yaml
  .pmcro/policies/                       <- execution, security, permissions, network
  .pmcro/trails/  .pmcro/frames/  .pmcro/evidence/  .pmcro/evaluation/  .pmcro/agent-memory/
  .pmcro/workflows/declarative/pmcro-cycle.yaml
  plugins/{pmcr-o,copilotkit,github,prompt-engineering,skill-creator}/
  docs/maf-native-architecture.md, docs/architecture/*.md
  output/                                <- regenerable scratch only (gitignored); never durable
```

## Related Repos (Z: drive)

| Repo | Role |
|------|------|
| `pmcro-runtime` | Aspire host + frontend (compiled runtime; own trail writer, `FileTrailWriter.cs`) |
| `dotnet-skills` | .NET / C# domain pack (blazor, aspire, maf plugins) |
| `agent-skills` | Base agentskills.io template |
| `figma-skills` / `github-skills` | Other domain packs |
| `casino-platform` | Separate product repo, not a skills/domain pack |

## Recent History (verify live before relying on this)

`aef75a7 fix: modernize Codex marketplace manifest` was the tip before
this rewrite. `77a27af feat: upgrade PMCRO runtime control plane and
provider boundaries` is the commit that introduced the current
`manifest.yaml`/`laws.yaml`/capability-provider-mcp architecture,
superseding the older `identity.json`/`catalog.json`/EC-law system this
file used to describe. `712ca70` and `46155bc` (2026-08-25) finished
wiring `policies/permissions.yaml`, `policies/network.yaml`, and the
secrets/config README updates into that architecture, and cleaned up
superseded `output/` scratch artifacts.

## Current Active-Work Snapshot

**Corrected 2026-08-26**: 8 completed trails and 1 failed trail exist
under `.pmcro/trails/`. `.pmcro/queue/pending/` holds one unclaimed
high-priority seed (`c4a8e2f1...`: draft `L-PLUGIN-ISOLATION`,
restricting TYPE-1-capable plugins from running without an active
Binding Envelope/open cycle). Still verify live before trusting any
narration, including this one — but don't assume the queue/trails are
empty without checking.

---

**Sources:** claude.ai account data export (Settings → Account → Export
data), `projects/01a038f0-a27f-7158-b826-09beb82e83be.json`, project
"PMCRO", attached doc `CONTEXT.md`, imported verbatim 2026-09-18.
