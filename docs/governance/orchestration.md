# PMCR-O as a MAF orchestration pattern

Microsoft Agent Framework names five standard ways to orchestrate multiple agents: **Sequential**,
**Concurrent**, **Handoff**, **Group Chat**, and **Magentic** (a lead "manager" agent that
dynamically directs a team based on evolving context — the same shape as Group Chat, but with a
planning manager choosing who acts next). Naming which one PMCR-O actually is clarifies what it
deliberately is not.

## PMCR-O is a bounded Handoff on a Sequential backbone

The default path — Planner → Maker → Checker → Reflector, in that order — is **Sequential**: each
role processes the trail and passes it forward, exactly like MAF's sequential pattern (agent A's
output becomes agent B's input).

The loop is where it stops being purely sequential. On a `LOOP` verdict, control does not continue
forward — it hands back to an earlier phase (a new `NN-plan.jsonl` / `NN-make.jsonl` pair on the
same trail). Responsibility shifting based on a condition or outcome is exactly MAF's **Handoff**
pattern definition. So PMCR-O's real shape is: a fixed sequential order, with one conditional
handoff edge (Checker → Planner/Maker on `LOOP`) that is bounded by Law 4 (MaxLoops = 3) rather than
open-ended.

## Why the Orchestrator is not a Magentic manager

Magentic's manager *chooses* which agent acts next based on its own reasoning about evolving
context — that is the whole point of the pattern, and it is what makes Magentic well-suited to
open-ended tasks with no known solution path. `AGENTS.md` and `plugins/pmcro/agents/orchestrator.agent.md`
give the Orchestrator a narrower job on purpose: it "owns lifecycle, trail open/seal, and phase
dispatch" and routes a **fixed** order, never picking the next role by judgement. The only decision
point in the whole cycle is the Checker's verdict, and even that is constrained to three literal
values (`PASS | LOOP | HALT`) — not a free-form plan.

This is a considered trade-off, not a missing feature: Magentic buys flexibility for open-ended
problems at the cost of an unpredictable execution path, which is hard to audit. PMCR-O buys full
auditability (every trail is the same five-phase shape, every loop-back is the same one edge) at
the cost of not handling problems whose solution path can't be decided in advance. If a future
template needs Magentic-style dynamic delegation, it should be a distinct workflow kind
layered *on top of* a sealed PMCR-O trail (e.g. a Maker phase that internally runs a Magentic
sub-orchestration and reports back one evidence bundle) — never a replacement for the Checker's
fixed verdict contract, or Law 3 stops meaning anything.

## Where Concurrent and Group Chat could still fit

Nothing here rules out a **Concurrent** Maker (e.g. multiple Makers attempt the same plan in
parallel and the Checker picks the best evidence) or a **Group Chat** Planner (several specialists
converge on one plan before Making starts). Both are extensions *within* a single PMCR-O phase, not
changes to the five-phase backbone or the loop-back edge — keep them scoped that way so a trail
stays comparable to every other trail regardless of which phase used which internal pattern.

## The same shape in Claude Code's own native mechanisms

Claude Code has four native ways to run more than one agent at once — subagents (per-turn,
caller-held plan), agent view (independent long-running sessions in their own worktree), agent teams
(peers with a shared task list, debating), and dynamic workflows (a scripted fan-out over many
items). They map onto the same patterns above without needing MAF at all: a **Concurrent** Maker is
`parallel()` in a workflow; a **Group Chat** Planner is an agent team told to challenge each other's
proposals before Making starts. Don't build a parallel "PMCR-O runs on Claude Code" spec for this —
it's the same Sequential-backbone-with-a-bounded-Handoff shape either way; only the execution
substrate differs.

Sources: [Agents — workflows and orchestration patterns, Microsoft Learn](https://learn.microsoft.com/dotnet/ai/conceptual/agents), [Workflows Orchestrations: Concurrent](https://learn.microsoft.com/agent-framework/workflows/orchestrations/concurrent), [Workflows Orchestrations: Magentic](https://learn.microsoft.com/agent-framework/workflows/orchestrations/magentic), [Microsoft Agent Framework overview](https://learn.microsoft.com/agent-framework/overview/), [Orchestrate agent teams — Claude Code Docs](https://code.claude.com/docs/en/agent-teams), [Orchestrate dynamic workflows — Claude Code Docs](https://code.claude.com/docs/en/workflows).
