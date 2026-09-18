---
name: pmcro-govern
description: >
  The single executable activation path for a PMCR-O cycle: open or resume a trail on
  disk, drive it through Plan -> Make -> Check -> Reflect, seal a disposition, and pause
  for human confirmation on any Type1-gated action. USE FOR: actually running a governed
  cycle by hand (Tier 0/2 executor, no C# runtime required) — this is what a session
  reads when it needs to open a new trail or resume one it finds already open on disk,
  so a chat session hitting its limit hands off via the trail, not a chat export.
  DO NOT USE FOR: the laws themselves (see docs/governance/laws.md) or per-role prompt
  detail beyond what's needed to write a phase's frame (see plugins/pmcro/skills/{role}
  and plugins/pmcro/agents/{role}.agent.md, which this consolidates for hand-execution).
---

# pmcro-govern

## What this is, honestly
- `plugins/pmcro/skills/pmcro-loop` plus the five `plugins/pmcro/skills/{orchestrator,
  planner,maker,checker,reflector}` skills describe the same five-phase loop, but assume
  a `TrailService.OpenFrameAsync(...)` C# call — per `.pmcro/tool-reference.md`, that
  class is real but **no phase agent calls it**; the runtime phases are stubs that always
  emit a `PENDING` frame.
- This skill is the Tier 0/2 hand-executable version (`docs/governance/executor-tiers.md`):
  a Claude session or a human with direct filesystem access runs the exact same five
  phases by literally writing the JSONL files below — no C# runtime required. It does not
  replace `pmcro-loop` or the per-role skills; it is what actually gets run today, since
  the runtime doesn't yet.
- The trail is the product: a cycle run this way produces the same sealed artifact a
  future runtime cycle would, and — because it's just files under `.pmcro/trails/` — is
  exactly the "handoff on disk instead of a chat export" continuity mechanism.

## Before anything: read
1. `docs/governance/laws.md` — the laws (EC-SYS-003, EC-VERIFY-FIRST-001, EC-004, EC-009,
   EC-PORTABLE-005, LAW-010; see the gap note on `PLAN-001` below).
2. `docs/governance/earned-constraints.md` — lessons already paid for; inherit them
   instead of relearning them.
3. `AGENTS.md` — repo-wide contract; re-verify the repo layout it describes against live
   disk, since it's explicitly allowed to drift.

## Step 0 — Resume-or-open
A trail is **open** if its directory has `00-frame.jsonl` and no `disposition.json`
(same test `plugins/pmcro/hooks/guard-log-before-act.js` uses to gate writes). Before
opening a new one:
- List `.pmcro/trails/{type}/` — `type` is usually `orchestrator` for a Claude-driven
  cycle, or a bare UUID at the trail root for the older style — and look for a directory
  matching this seed's intent that is still open.
- If found: **resume**. Read every `NN-*.jsonl` already present, determine the highest
  phase reached and the current loop number, and continue from the next phase. Do not
  re-open or rewrite `00-frame.jsonl` — it's append-only (Law 10).
- If not found: open one — see Step 1.

This is the actual point of this skill: it's what lets a new chat session pick up a
cycle a previous session started, from the trail on disk, with no pasted-in context
required.

## Step 1 — Open (`00-frame.jsonl`)
Create `.pmcro/trails/{type}/{slug-or-uuid}/00-frame.jsonl`, one JSON line, before any
other mutation (EC-SYS-003). Minimum fields:
```
{"ts": "<ISO8601>", "role": "orchestrator", "phase": "frame", "seed": "<one-line task>",
 "true_intent": "<why, and what continuity/acceptance this cycle needs>",
 "workspace": ".", "max_loops": 3,
 "type1_gates": ["git commit or push", "deleting or moving anything in the source repo"]}
```
Every path recorded inside every frame is relative to the workspace root
(EC-PORTABLE-005) — never `T:\...` or `C:\Users\...`. The tool call that writes the file
needs an absolute host path; the JSON *content* never does.

## Step 2 — Plan (`{NN}-plan.jsonl`)
Act as Planner. Append one line: a short decision, the evidence it's based on (files
actually read, commands actually run — not assumed), a numbered `steps` list, and
`acceptance` criteria. Planner does not touch files outside `.pmcro/`.

## Step 3 — Make (`{NN}-make.jsonl`)
Act as Maker. Execute the plan's steps. **Before executing any step that matches a
`type1_gates` entry from the frame, stop and ask the human to confirm — do not perform a
Type1-gated action unattended, no matter how minor it looks.** Every non-gated step still
needs real evidence per EC-VERIFY-FIRST-001 (paths written, byte counts, command stdout)
— no "should work". Maker never emits a verdict (EC-004).

## Step 4 — Check (`{NN}-check.jsonl`)
Act as Checker. Independently verify each acceptance criterion from the Plan against
what the Maker actually produced — re-read the file, re-run the command, don't trust the
Maker's own evidence line unchecked. Emit exactly one of `PASS | LOOP | HALT`, nothing
else (EC-004).

## Step 5 — Reflect / Seal
- **PASS** → write `disposition.json` (shape below) with `"disposition": "ACCEPT"`. If
  this trail looped at all before reaching PASS, append the durable lesson to
  `docs/governance/earned-constraints.md`, one line, citing this trail — only now, never
  on a mid-loop `LOOP` verdict.
- **LOOP**, and current loop count < `max_loops` (from the frame, default 3 / EC-009) →
  Reflector notes what the Checker rejected, bump `NN`, go back to Step 2 on the *same*
  trail directory. Never open a new `00-frame.jsonl` for a loop-back (Law 10).
- **HALT**, or loop count has reached `max_loops` → write `disposition.json` with
  `"disposition": "HALT"`, stop, wait for a human. Do not retry past `max_loops`
  (EC-009) — non-negotiable, not a suggestion.

`disposition.json` shape (standardizing on the fuller of two shapes seen in this repo's
own sealed trails):
```
{"cycle_id": "<trail dir name>", "disposition": "ACCEPT|HALT",
 "checker_verdict": "PASS|HALT", "sealed": true, "reason": "<one line>",
 "timestamp": "<ISO8601>"}
```

## Type1 gates — pause, don't perform
A Type1 gate is any action listed in the frame's `type1_gates` array. As of this cycle
that always includes **git commit or push**, and this repo's migration-shaped cycles
also name **deleting or moving anything in the source repo**. When a planned step
matches one: write the plan step normally, but in Make, stop *before* executing it,
state exactly what command or change is about to run, and wait for explicit human
go-ahead in the current session before continuing. This is Tier-2 human-as-hands per
`docs/governance/executor-tiers.md`, scoped to just the gated step — everything else in
the same cycle can still run unattended.

## Known gaps this skill does not paper over
- `docs/governance/laws.md` does not currently define `PLAN-001`, even though
  `AGENTS.md` and `.pmcro/tool-reference.md` both cite it as a real, enforced law
  (schema-valid JSONL, no `<placeholder>`). This skill follows `PLAN-001` in practice
  (see the frame/plan/make shapes above) but flags the doc gap rather than silently
  patching `laws.md` outside its own review.
- Existing sealed trails in this repo do not share one JSONL schema — e.g.
  `d6f245c2-020a-4423-97e7-96c05a0c827e`'s frame uses `cycle_id`/`seed_intent`/
  `truest_intent`; `git-governance-fix-20260917`'s uses `cycle_id`/`seed_intent`/
  `actor`; this skill's own trail (`build-pmcro-govern-20260918`) uses `ts`/`seed`/
  `true_intent`/`host`. Old trails are sealed and append-only (Law 10) and are never
  rewritten to match. The shapes in this skill are what *new* trails should converge on
  going forward, not a retroactive standard.

Next: this skill's own trail (`.pmcro/trails/orchestrator/build-pmcro-govern-20260918/`)
is its first real run — read that trail's `01-check.jsonl` and `disposition.json` before
trusting this skill on a second, unrelated cycle.
