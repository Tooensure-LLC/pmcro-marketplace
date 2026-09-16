# Parking lot

Named ideas that came up in design conversation but aren't built here — kept so they aren't lost,
not because they're queued for implementation. Move an item out of this file only when there's an
actual concrete step to take on it.

## Not ready to design yet

The three items below were guessed at from a garbled voice-transcribed message before their source
was found. A vision note captured the same day in the `PMCRO-AI-Agent-Company/AgentSkills.Template`
project (`claude/vision-note-orchestrator-service-and-llm-federation.md`, 2026-09-06) gives the real
shape — corrected here, not because it changes what's built (still nothing), but because acting on
the wrong shape later would be worse than acting on none:

- **Orchestrator Service** — running *multiple competing Orchestrators* against the same task at
  once, each free to pick its own strategy, with a "referee" role above them to adjudicate conflicts
  and prevent sabotage/copying. Pitched as a pay-as-you-go commercial model: more budget buys more
  competing Orchestrators and a faster/better result. Explicitly not scoped, not designed, and the
  idea's own originator raised — and explicitly deferred — a separate open question about whether
  the current Orchestrator/Planner/Reflector split is even right, saying outright "I don't know, I'm
  gonna leave that." Treat the existing `plugins/pmcro/agents/*.agent.md` contract as authoritative
  until that's revisited deliberately.
- **LLM Federation** — a layer *above* the Orchestrator Service: a simulated roundtable of multiple
  perspectives/personas discussing a problem together, explicitly domain-agnostic (not a fixed cast
  of roles). Open question raised but not resolved: whether the existing trail mechanism is even the
  right substrate for recording a simulated multi-perspective discussion.
- **Autonomous-in-the-loop** — real thesis (`claude/thesis-human-in-loop-enables-autonomous-2026-09-07.md`),
  stated three times in one session: "Human in the loop enables autonomous in the loop. Human in the
  loop generates training data." The genuinely distinctive part isn't the loop itself (imitation
  learning / process supervision already cover that) — it's that PMCR-O's governance rules produce
  training-shaped data as a *byproduct*: failed attempts survive with their verdicts attached instead
  of being discarded, Earned Constraints are a learned prior already in text form, verdicts are
  machine-parsed rather than judged, and sealed trails are immutable (exactly the provenance property
  training data needs and rarely has). The same note raises a serious unresolved hole worth reading
  in full before building anything on this thesis: **a trail can be procedurally perfect and
  epistemically empty** — every gate can fire correctly on criteria that were written so they could
  only ever pass. A bad trail isn't a failed trail (a failed, honestly-sealed trail is valuable); a
  bad trail is a dishonest one, and "spotless" should read as suspicious, not reassuring. The
  concrete failure mode: someone wants to believe something (the source doc's own example is
  someone convinced they'd seen aliens), runs a feedback loop with an AI, and gets back a trail that
  confirms it — procedurally clean, epistemically worthless. **This is treated as load-bearing, not
  hypothetical** — it's the specific reason "trail marketplace" needs a real accountability layer
  before it can be a marketplace at all, not an afterthought bolted onto a working PASS/LOOP/HALT
  gate. That source project scores trail quality on four checkable properties before anyone reads
  the subject matter — falsifiable, grounded, challenged, cost something — as a partial answer.
  Nothing here has been brought into this repo's laws or trail format yet; do that deliberately, as
  its own Planner phase, before any feature that lets a trail be published or sold.
- **GitHub as Agent Playground** — using GitHub Actions/Issues as the substrate agents operate in.
  Not scoped beyond the name; not found in the mined project docs either.

## Explicitly not adopted (and why)

A separate old design session built out a much heavier version of this same repo — a `pmcro-csuite`
plugin (Chief roles: CTO/CPO/CISO/etc.), a 35-skill "Reasoning Skills Catalog," a `skill-creator`
plugin, and a root-level `schemas/` + `scripts/` "marketplace hub" — before that same session's
author called it overbuilt and collapsed it back down to the lean tree this repo actually is. None
of that is being rebuilt here:

- It contradicts the standalone-first principle in `AGENTS.md` (pillar 1 must stay markdown/JSON
  only, readable and runnable by hand by any frontier LLM).
- A "reasoning skill catalog" and a "strategy" layer on top of the existing five-phase loop is
  exactly the kind of boundary-collapse this repo's laws exist to prevent — PMCR-O's power is being
  the same five phases every time, not a configurable cognition stack.
- The one genuinely reusable idea from that design — keeping external-framework references
  separate from local implementation, with a ground-truth disclaimer and sourced URLs — is kept,
  formalized as a convention rather than a plugin: see `.pmcro/reference-convention.md`.

If a real need for any of this shows up (not just "it would be nice to have"), redesign it small and
prove it against one real trail before generalizing — don't restore the old shape wholesale.

## Open reconciliation with pmcro-runtime (real repo, not speculative)

`pmcro-runtime` (https://github.com/ShawnDelaineBellazanLoop/pmcro-runtime) is a real, working
.NET repo, more mature in its governance domain layer than anything in this repo. Two concrete
gaps surfaced comparing them, neither resolved yet:

- **Earned Constraints format.** `pmcro-runtime` models a constraint as a JSON entity
  (`EarnedConstraint` + `EarnedConstraintRepository`) at `.pmcro/trails/constraints/earned-constraints.json`,
  with a promotion lifecycle (candidate → promoted → suspended → retired) gated by
  `laws/constraint-promotion.md` — update is `approval`, delete is `deny` (retire, don't erase).
  This repo has a flat markdown list at `.pmcro/earned-constraints.md` with no lifecycle. Don't
  silently convert one to the other; this needs a decision (do earned constraints live in the
  runtime only, with this repo just documenting the *concept*, or does this repo need the same
  JSON shape to stay consistent?).
- **Missing Python control plane.** `.pmcro/policies/resource-operations.json`'s own description
  says it's read by "the Python control plane in pmcro-skills." No such control plane exists in
  this repo. Either that line in the shared policy file is aspirational/not yet true, or there's
  a third piece of this system nobody has started. Worth asking directly rather than assuming.

## pmcro-code implementation notes (not this repo)

For whenever `pmcro-code`'s runtime actually gets built (there is no C# project in this repo today):

- Base it on the **Ardalis Clean Architecture** shape (`BaseEntity` with `Guid.NewGuid()`,
  `IRepository<T>`, a Result-pattern rather than exceptions) rather than Jason Taylor's
  MediatR-heavy/CQRS variant — it's the closer match to a `TrailService`-style domain and avoids a
  leaky `IApplicationDbContext`.
- Structure each OMode / cycle type as a **Vertical Slice** (`Features/Trails/OpenTrail/`,
  `Features/Trails/WriteFrame/`, `Features/Trails/SealTrail/` — command + handler in one folder)
  rather than spreading one cycle across Clean Architecture's horizontal layers. This maps directly
  onto Planner → Maker → Checker → Reflector: each phase is naturally one slice.
- Use `Result<T>` for the Checker's `PASS | LOOP | HALT` verdict instead of throwing — a verdict is
  an expected outcome, not an exceptional one.
- If a pipeline behavior enforces Law 1 (Log Before Act) automatically, keep it as one MediatR
  pipeline behavior wrapping every mutating command — not scattered manual checks — so it can't be
  bypassed by a new command forgetting to call it.
