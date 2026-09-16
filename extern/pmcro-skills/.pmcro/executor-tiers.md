# Executor tiers

A concept from a sibling project (`PMCRO-AI-Agent-Company/AgentSkills.Template`,
`claude/design-note-executor-tiers-2026-09-07.md`) that fits this repo's own standalone-first
principle well enough to be worth stating here explicitly, even though nothing below is wired into
this repo's trail format yet — see `.pmcro/reference-convention.md` for how to read a note like this.

## The core idea

A trail frame is a *portable execution unit*, not a description of one. Handing the same frame to
something with no filesystem access (a browser-bound LLM, a human with no dev environment) doesn't
make the frame fail — it can still reason within the frame and emit what would have to be done. What
changes across executors is privilege, not the frame's validity. **The executor is a variable of the
frame, not a property of it.**

## The four tiers

| Tier | Who acts | What it emits | Evidence returns via |
|---|---|---|---|
| 0 — privileged | An agent with direct tool access | The change itself | Its own writes |
| 1 — dispatched | An agent via an actuator | An `Execute*` call under Orchestrator authority | The actuator's result |
| 2 — human-as-hands | A human | The exact command/script and what must exist afterward | The human pastes the output back |
| 3 — foreign runtime | Another LLM or agent with no filesystem | The frame itself, reasoned over | Whatever that runtime can prove |

Tier 2 is the one most designs skip, on the assumption an agent needs its own hands. It doesn't — it
needs a frame and someone willing to be the hands. No API key, no terminal access, no GitHub token,
no budget: this is the zero-cost adoption path, and it produces a trail identical in shape to a Tier
0 trail. This is the same property `AGENTS.md`'s standalone-first principle already relies on — a
frontier LLM with no tools can still read `SKILL.md` and act out a cycle by hand — Tier 2 just names
the mechanism precisely: a human relaying that LLM's exact instructions back into the workspace.

## What would need to change to wire this in

One field, not a subsystem: `executed_by` (`agent | actuator | human | foreign`) on a Maker's
evidence. Tier 2 additionally records the emitted command verbatim, so verification still has
something falsifiable to check. The Checker's contract is untouched either way — a verdict is still
exactly `PASS | LOOP | HALT`.

Not yet decided, and not this repo's call to make unilaterally: whether Tier 3 evidence is admissible
in a sealed trail at all, or only citable as a reference.
