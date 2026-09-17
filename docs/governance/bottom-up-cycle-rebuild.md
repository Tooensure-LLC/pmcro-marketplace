# Bottom-up cycle rebuild (2026-09-17)

Recorded after a design session on `pmcro-marketplace` overbuilt toward an autonomous
"AI Agent Company" (message queue, checkpointing, self-fine-tuning) and deliberately stepped back
down: build the actual loop first, small, before anything runs unattended. This is the decided
shape to build up from — not a parked idea, a plan with concrete next steps below.

## The decided shape

- **Orchestrator** — the only stateful node. Owns high-level goals, the Laws (`laws.md`),
  C-suite context, and **the message queue**. Nothing else persists across cycles.
- **Planner** — stateless. Plans the bare minimum, based on validated resources plus whatever
  high-level goal Orchestrator handed it. No more, no less.
- **Maker** — executes the plan, cites evidence (`EC-VERIFY-FIRST-001`), never scores itself
  (`EC-004`).
- **Checker** — verdict only: `PASS | LOOP | HALT`. Unchanged from today.
- **Reflector** — on a non-`PASS` verdict, does not just record a disposition. It reformulates the
  Checker's verdict + evidence into a **next seed intent** and hands it back.
- **Loop closes through Orchestrator, not Reflector directly** — Reflector's next seed intent goes
  into Orchestrator's queue. Orchestrator decides when/whether to dispatch it, not Reflector.

This is compatible with `orchestration.md`'s existing analysis (Sequential backbone, one bounded
Handoff edge on `LOOP`, capped by `EC-009`) — it doesn't change that shape, it specifies *how* the
handoff edge actually closes: through a queue Orchestrator owns, not by silently reopening the same
trail.

## Gap vs. what's actually built (pmcro-marketplace, .NET/MAF, confirmed 2026-09-17)

- `OrchestratorGrpcService.Run` is pure intent-routing triage today — no queue, no state held
  across calls. Every call is stateless single-shot via `MafPhaseRunner`.
- `ReflectorGrpcService.RunCycle` returns `ACCEPT | HALT` only. It does not generate a next seed
  intent on failure — that field doesn't exist yet.
- `EC-009` MaxLoops is a fixed count (3), not a resource/time budget. Fine for a bounded single
  trail; not sized for "runs all day."
- No chained-cycle driver exists at all — `OrchestratorController.cs`'s own doc comment already
  says so: "There is currently no endpoint that runs a full cycle."

## Todo

- [ ] Give Orchestrator persistent state: a queue of pending seed intents, plus wherever
      Laws/C-suite context actually lives (currently nowhere — it's baked into each phase's
      system prompt string in `MafPhaseRunner.RunAsync` callers)
- [ ] Add a next-seed-intent field to Reflector's response, populated on non-`PASS`
- [ ] Orchestrator accepts that next seed intent and enqueues it, instead of the caller/facade
      discarding it (today: `OrchestratorApi` callers just get the disposition back and the trail
      ends there)
- [ ] Build the actual chained-cycle driver: dequeue → Orchestrator → Planner → Maker → Checker →
      Reflector → back to queue. This is the literal missing piece between "five stateless
      facades" and "a loop"
- [ ] Evolve `EC-009` from a fixed loop count into a resource/time budget + escalation path,
      *before* anything runs unattended — an unbounded "runs all day" loop is exactly the failure
      mode `EC-009` exists to prevent, and it needs to grow with the design, not get bypassed
- [ ] Open: queue backing store (in-memory vs Postgres vs Redis) — not decided
- [ ] Open: where Orchestrator's Laws/C-suite state actually persists between cycles — not decided

## Explicitly deferred, not forgotten

- **Self-fine-tuning on trail frames.** Legitimate idea, matches the "Autonomous-in-the-loop"
  thesis already in `parking-lot.md`. Not wired into the live loop until an eval-gate + rollback
  exists — the thing training the next version of itself unsupervised is a different risk class
  than anything else on this list. Offline pipeline first, human-gated, once the loop above
  actually runs.
- **Everything in `parking-lot.md`'s "Not ready to design yet" and "Explicitly not adopted"
  sections** — still not being resurrected. This rebuild stays scoped to the five-phase loop plus
  a queue; it is not a re-opening of `pmcro-csuite`, the Reasoning Skills Catalog, or Orchestrator
  Service/LLM Federation.
