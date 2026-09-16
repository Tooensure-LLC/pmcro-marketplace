# CLAUDE.md

@AGENTS.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository. Claude Code loads `CLAUDE.md` automatically but does not load `AGENTS.md` on its own — the `@AGENTS.md` import above is what actually pulls the five laws and root contract into context for a Claude Code session; don't remove it.

## What this repo is

This is not an application — it's the source for **pmcro**, a portable "PMCR-O loop" (Planner → Maker → Checker → Reflector, under an Orchestrator) that gets distributed as an agent skill/plugin to multiple AI coding hosts (Claude Code, Cline, Codex, Cursor). There is no build, lint, or test tooling; the deliverable is the markdown/JSON skill and agent definitions themselves, plus a JSON-lines audit trail format ("trails") that any host can write to and inspect.

## Repo layout and why it's duplicated

The same content is intentionally repeated under several host-specific paths because each host discovers skills/plugins differently:

- `plugins/pmcro/` — the canonical Claude-compatible plugin: `plugin.json`, `skills/pmcro-loop/SKILL.md`, `skills/pmcro-loop/references/laws.md`, and `agents/*.agent.md` (one file per role: orchestrator, planner, maker, checker, reflector).
- `.claude-plugin/marketplace.json`, `.codex-plugin/marketplace.json`, `.cursor-plugin/marketplace.json`, `.agents/plugins/marketplace.json` — identical marketplace catalogs (same shape) that all point at `./plugins/pmcro`. If you change one, check whether the others need the same change (compare with `diff`) — they are meant to stay byte-identical.
- `.cline/skills/pmcro-loop/` — a standalone copy of the skill for Cline, which uses Agent Skills paths instead of a Claude-style marketplace. Its `SKILL.md` and `references/laws.md` mirror `plugins/pmcro/skills/pmcro-loop/` and must be kept in sync manually when the loop logic changes.
- `AGENTS.md` — the root contract every host/agent must follow (the five laws, skill path table per host, roles directory, laws pointer). Treat this as the authoritative summary; `.clinerules` just defers to it.
- `.pmcro/laws.md` — the canonical text of the five non-negotiable laws (also duplicated into `references/laws.md` in both skill copies for self-containment).
- `.pmcro/trails/` — where every executed PMCR-O cycle writes its audit trail (see below). `.pmcro/trails/working/` holds in-progress/example trails.

When editing skill behavior, update `plugins/pmcro/skills/pmcro-loop/SKILL.md` and mirror the change into `.cline/skills/pmcro-loop/SKILL.md`. When editing the laws, update `.pmcro/laws.md` and both `references/laws.md` copies.

## The five laws (`AGENTS.md`, `.pmcro/laws.md`)

1. **Log Before Act** (`EC-SYS-003`) — open a trail frame before any file mutation.
2. **Verify First** (`EC-VERIFY-FIRST-001`) — the Maker must cite real evidence (command output, diffs), never "should work" claims.
3. **Checker Only** (`EC-004`) — the Maker never self-scores; only the Checker issues a verdict.
4. **MaxLoops** (`EC-009`) — at most 3 loops per trail, then HALT to a human.
5. **Relative Paths Only** (`EC-PORTABLE-005`) — every path recorded in a trail is relative to the workspace root; no absolute host paths. A trail must stay portable enough to share, replay elsewhere, or use as fine-tuning data.

## The PMCR-O cycle (`plugins/pmcro/skills/pmcro-loop/SKILL.md`)

1. **Open trail** — create `.pmcro/trails/{uuid}/00-frame.jsonl` with the seed and true intent, before any mutation.
2. **Plan** — as Planner, write `01-plan.jsonl` with a minimum plan and Checker-verifiable acceptance criteria.
3. **Make** — as Maker, execute the plan and write `01-make.jsonl` with artifacts and evidence.
4. **Check** — as Checker, verify against the plan and write `01-check.jsonl` with a verdict: `PASS | LOOP | HALT`.
5. **Reflect / Seal**:
   - `PASS` → write `disposition.json` with `"disposition": "ACCEPT"`, stop.
   - `LOOP` and loop count < 3 → Reflector writes `NN-reflect.jsonl` with the learning, next cycle continues on the *same* trail with incremented file numbers (`02-plan.jsonl`, `02-make.jsonl`, ...).
   - `HALT` or loop count ≥ 3 → `disposition.json` with `"disposition": "HALT"`, stop for a human.

Each role is defined in `plugins/pmcro/agents/{role}.agent.md` and has a narrow, non-overlapping mandate — e.g. the Orchestrator only dispatches phases and never does domain work; the Checker only issues verdicts and never repairs the Maker's output. When acting as a given role, follow that role's file exactly rather than blending responsibilities.

See `.pmcro/trails/5a957afe-d7f7-49dc-b4ed-66adccf39a3c/` for a complete real example of a sealed trail (frame → plan → make → check → reflect → disposition), `.pmcro/trails/working/` for examples of a trail that looped (`02-*` files) before sealing, and `.pmcro/trails/f692dba3-9f4d-47cb-bbbb-260e4d210df9/` for the trail that built `plugins/pmcro/hooks/` (see below).

## The one place this repo does real computation

`plugins/pmcro/hooks/hooks.json` + `hooks/guard-log-before-act.js` enforce EC-SYS-003 (Log Before Act) via a real Claude Code `PreToolUse` hook — not documentation, an actual block. This is a deliberate, narrow exception to "pillar 1 does no computing itself" (see `AGENTS.md`'s Product pillars section); it doesn't gate Bash calls (stated limitation), and only takes effect for a Claude Code session that installs this plugin.

## This repo is not the product

See `AGENTS.md`'s "Product pillars" section for the full, verified picture. Short version:
**`Tooensure-LLC/pmcro-marketplace`** (a separate, real .NET solution already under local
development) is the actual product; this tree has been folded into it at `extern/pmcro-skills/` as
reference content, not pushed anywhere on its own. `pmcro-runtime`, `PMCRO-AI-Agent-Company/AgentSkills.Template`,
and `ShawnDelaineBellazanLoop/pmcro-skills` (an unrelated Python project — don't confuse it with
this tree) are three further real but distinct repos. `pmcro-desktop` is still just a name.

Pillar 1 must always work standalone: any frontier LLM should be able to read `SKILL.md`/`AGENTS.md` and run a full cycle by hand, producing the same trail files `pmcro-runtime` would. Never design a feature here that only works if pillar 2 or 3 exists.

## Trail file conventions

- Path: `.pmcro/trails/{uuid}/{NN}-{role}.jsonl` where `NN` is a two-digit loop counter (`00` reserved for the frame; `01`, `02`, `03`... per subsequent loop).
- Each line is a single JSON object, minimum fields: `ts` (ISO timestamp), `role`, `phase`, and the relevant `law` id being satisfied.
- A sealed trail ends with `disposition.json` at the trail root containing `disposition`, `verdict`, `loops`, `max_loops`, `sealed_at`, `sealed_by`, `artifacts`, and per-acceptance-criterion results.
- `.pmcro/trails/working/{uuid}/` is the convention for a trail still in progress (see the two examples already there). A `pmcro-desktop`-style sweeper may later pick up any `working/` trail once its `disposition.json` appears and move it elsewhere for long-term storage — but nothing in this repo depends on that happening, and a trail left in `working/` with no sweeper is still a complete, valid trail on its own.

## Templates (`templates/`)

Skills, plugins, agent roles, and whole products can be generated declaratively from a **template**
rather than hand-written — see `templates/README.md` for the concept and `templates/template.schema.md`
for the exact JSON shape (`vars`, `declares.files`, bound `laws`, optional `interception_points`).
Worked examples for each of the four template kinds (`skill`, `plugin`, `agent`, `product`) are in
`templates/examples/`. Generating from a template is still a mutation bound by the five laws above —
it must open a trail like any other change.

## MAF-native concepts (`.pmcro/interception-points.md`, `.pmcro/orchestration.md`)

pmcro's governance model is meant to map onto real Microsoft Agent Framework concepts, not just
borrow their names:

- `.pmcro/interception-points.md` — the eight Agent Hooks boundaries (`agent_startup`, `input`,
  `pre_model_call`, `post_model_call`, `pre_tool_call`, `post_tool_call`, `output`,
  `agent_shutdown`), grounded in which of MAF's three real middleware layers (agent/chat/function)
  each comes from, with onion/short-circuit semantics — a deny at `pre_tool_call` must actually
  prevent the tool call, not just log it. These map onto sub-lines within a trail's existing phase
  files (mostly `01-make.jsonl`) rather than replacing them.
- `.pmcro/orchestration.md` — names PMCR-O as a bounded Handoff (the Checker's `LOOP` verdict) on a
  Sequential backbone (Planner→Maker→Checker→Reflector), and explains why the Orchestrator is
  deliberately not a Magentic-style dynamic manager: PMCR-O trades open-ended flexibility for full
  auditability on purpose.

Read both before wiring approval gates, fine-grained logging, or any dynamic-delegation feature
into the Maker or Orchestrator — and verify against Microsoft Learn / agentskills.io rather than
extending from memory, since these docs cite specific, checkable pages.
