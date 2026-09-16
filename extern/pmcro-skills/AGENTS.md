# AGENTS.md — Root Contract

All agents and hosts (Claude Code, Cline, Codex, Cursor, etc.) MUST:

1. **Log Before Act** — open `.pmcro/trails/{uuid}/{NN}-{role}.jsonl` before any file mutation.
2. **Verify First** — Maker cites real evidence (command output, diffs), not claims.
3. **Checker Only** — Maker never self-scores. Only Checker issues PASS | LOOP | HALT.
4. **MaxLoops** — max 3 loops, then halt to human.
5. **Relative Paths Only** — every path recorded in a trail is relative to the workspace root; no absolute host paths (a trail is a portable product, not a local log).

## Run a cycle

Load the **pmcro-loop** skill and follow it.

| Host | Skill path |
|------|------------|
| Claude / Codex / Cursor | `plugins/pmcro/skills/pmcro-loop/SKILL.md` |
| Cline | `.cline/skills/pmcro-loop/SKILL.md` |

## Roles

`plugins/pmcro/agents/` — orchestrator, planner, maker, checker, reflector

## Laws

`.pmcro/laws.md`

## MAF-native concepts

pmcro's operations are meant to be understood in Microsoft Agent Framework's own terms, not just
inspired by them loosely:

- `.pmcro/interception-points.md` — the eight MAF Agent Hooks boundaries (`agent_startup`, `input`,
  `pre_model_call`, `post_model_call`, `pre_tool_call`, `post_tool_call`, `output`,
  `agent_shutdown`), which three real MAF pipeline layers (agent/chat/function middleware) they
  come from, and how they map onto a trail's phase files.
- `.pmcro/orchestration.md` — which MAF orchestration pattern (Sequential, Concurrent, Handoff,
  Group Chat, Magentic) PMCR-O actually is (a bounded Handoff on a Sequential backbone), and why
  the Orchestrator is deliberately not a Magentic manager.

Ground any further MAF work in the real docs (Microsoft Learn) and the real Agent Skills
specification (agentskills.io) before extending these — both are reachable as MCP tools in Claude
Code; don't guess at either format from memory. When writing up any such external framework,
follow `.pmcro/reference-convention.md`.

## Ideas not built here

- `.pmcro/parking-lot.md` — named but unimplemented ideas (kept so they aren't lost), and a short
  record of what was proposed for this repo and deliberately rejected as over-scope, with why.
- `.pmcro/executor-tiers.md` — a sibling-project concept (a trail frame's executor — agent, actuator,
  human, or a foreign LLM — is a variable of the frame, not a property of it) that reinforces this
  repo's own standalone-first principle; not wired into the trail format here yet.

## Product pillars — this local tree is not the product

As of 2026-09-16, **`Tooensure-LLC/pmcro-marketplace`** (real repo, real .NET solution already under
active local development — Domain/Application/Infrastructure Clean Architecture, real C# laws,
`PMCR.Agents`, an Aspire host) is the actual product. This local tree (`pmcro-skills`) has been
folded into it at `extern/pmcro-skills/` as reference/governance content — it is not pushed
anywhere on its own, and does not have its own canonical remote. Treat everything below as *what
this content is for*, not as a standalone product roadmap.

Repos that are real (verified against the GitHub API, not assumed):

- **[`Tooensure-LLC/pmcro-marketplace`](https://github.com/Tooensure-LLC/pmcro-marketplace)** — the
  product. Local checkout at `T:\pmcro-marketplace`, git remote confirmed, zero commits yet.
- **[`pmcro-runtime`](https://github.com/ShawnDelaineBellazanLoop/pmcro-runtime)** — a separate .NET
  execution engine (governance domain layer, `GovernedResourceAttribute` + CRUD policy table,
  `EarnedConstraint` as a first-class entity, a MAUI client). Not the same thing as `pmcro-marketplace`.
- **[`PMCRO-AI-Agent-Company/AgentSkills.Template`](https://github.com/PMCRO-AI-Agent-Company/AgentSkills.Template)** —
  a third, heavier "AI Agent Company" architecture effort (12-Chief C-Suite, 36-reasoning-strategy
  catalog, MCP actuator servers). Source of the mined design notes in `.pmcro/parking-lot.md`.
- **`ShawnDelaineBellazanLoop/pmcro-skills`** — exists, but is an **unrelated Python project**. Do
  not confuse it with this tree, and do not push this tree there.
- **`pmcro-desktop`** — still just a name from planning conversation, not a repo.

**Standalone-first still holds for this content specifically:** it must remain readable and
runnable by hand by any frontier LLM, with zero installation. Whether `pmcro-marketplace` itself
stays that lean is `pmcro-marketplace`'s own call, not this tree's — `pmcro-marketplace` already
includes a `pmcro-csuite` (12-Chief) plugin and a `pmcro-code` (skill-creator, meta-runtime, etc.)
plugin, both of which this tree's own `.pmcro/parking-lot.md` explicitly declined to build. That is
an open divergence, not yet reconciled, and not this file's call to resolve unilaterally.

One deliberate, small exception to standalone-first within *this* tree: `plugins/pmcro/hooks/hooks.json`
+ `hooks/guard-log-before-act.js` enforce EC-SYS-003 for real, for any Claude Code session that
installs this plugin — a `PreToolUse` hook, not prose. This is genuine computation, crossing the "no
computing itself" line on purpose, in one narrow place, with a real sealed trail behind it
(`.pmcro/trails/f692dba3-9f4d-47cb-bbbb-260e4d210df9/`). It does not gate Bash calls (stated scope
limitation, not an oversight) and is inert unless the pmcro plugin is actually installed.

**Known divergences with `pmcro-runtime`, not yet reconciled:**
- `pmcro-runtime`'s `.pmcro/policies/resource-operations.json` (mirrored here) expects a "Python
  control plane in pmcro-skills" that doesn't exist anywhere yet.
- `pmcro-runtime` models Earned Constraints as a JSON entity with a promotion lifecycle at
  `.pmcro/trails/constraints/earned-constraints.json`; this tree has a flat markdown list at
  `.pmcro/earned-constraints.md`. See `.pmcro/parking-lot.md`.
- Law numbering: see "Reconciliation with pmcro-runtime" in `.pmcro/laws.md`.