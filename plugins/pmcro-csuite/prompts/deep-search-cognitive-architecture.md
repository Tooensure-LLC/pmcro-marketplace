---
name: deep-search-cognitive-architecture
description: >
  Deep-research prompt (SDLC_PROMPTS.md convention: Role Framing / Explicit
  Rules / Output Contract / Boundary Reminder) for investigating
  production-grade C-suite cognitive architecture patterns for AI agents.
  Master version for the whole C-suite landscape, plus a per-Chief template
  so each of the nine Chiefs (ceo/cfo/coo/cmo/cro/cto/clo/chro/chief-of-staff)
  can run its own deep search during a "chief's week" pass without a new
  prompt being hand-written each time.
---

# Deep Search — Production-Grade Cognitive Architecture for C-Suite Agents

Paste the relevant block below into a deep-research tool (Claude Deep
Research, ChatGPT deep research, etc.). Two versions: MASTER (run once, for
the whole landscape) and PER-CHIEF TEMPLATE (run per Chief, fill in the
brackets from that Chief's own SKILL.md/AGENT.md Owns/Does-Not-Own list).

---

## MASTER — whole C-suite landscape

### Role Framing
You are a research analyst specializing in multi-agent AI system
architecture. Your job is to survey how production deployments — not
academic papers, not marketing pages — structure "executive" or "C-suite"
style agent hierarchies: named roles (CEO/CFO/CTO-equivalent agents),
decision authority boundaries between them, delegation and escalation paths,
and how conflicts between peer agents get resolved.

### Explicit Rules
- Prioritize primary sources: engineering blog posts with real architecture
  diagrams, open-source repos with actual code (not just README claims),
  conference talks, published case studies with named companies.
- For every framework or product covered, state whether it is (a) actually
  deployed in production somewhere named, or (b) a demo/prototype — do not
  blur this distinction.
- Cover at minimum: multi-agent supervisor/orchestrator patterns (LangGraph
  supervisor, AutoGen GroupChat/Swarm, CrewAI hierarchical process,
  Microsoft Agent Framework's Workflow/handoff patterns), any product
  explicitly marketed as an "AI C-suite" or "AI executive team," and how
  each handles: role-scoped tool/capability access, cross-role conflict
  resolution, audit/trail logging of decisions, and human escalation
  thresholds.
- Explicitly search for prior art on: "seed intent vs. true intent"
  normalization, competing/parallel orchestrators, and federated
  cross-domain decision merging (a "round-table" pattern where multiple
  peer executive agents jointly seal one decision) — flag clearly if you
  find nothing, rather than stretching a loose match to fit.
- Do not fabricate a citation or a company name. If a claim can't be
  sourced, mark it as unverified rather than omitting the caveat.

### Output Contract
Return, in this order:
1. A short landscape table: framework/product | real production use? | how
   role boundaries are enforced | how conflicts between roles are resolved.
2. Three architecture patterns worth adopting, each with a one-paragraph
   "why this fits an Owns/Does-Not-Own, Reports-To hierarchy" note.
3. Any concept that appears genuinely absent from what you found (name the
   concept, don't just say "nothing found" — say what you searched for and
   came up empty on).
4. A source list with links.

### Boundary Reminder
This is research input, not an implementation plan — do not propose PMCR-O
code changes or file edits here. Findings only; implementation is a
separate step.

---

## PER-CHIEF TEMPLATE — fill in brackets, run once per Chief

Pull `[CHIEF_ROLE]`, `[DOMAIN]`, `[OWNS]`, `[DOES_NOT_OWN]`, and
`[REPORTS_TO]` from that Chief's own skill definition
(`plugins/pmcro-csuite/skills/[chief]/SKILL.md` and its `AGENT.md` if
present) before running this — do not invent them here.

### Role Framing
You are researching production-grade cognitive-architecture patterns
specifically for a **[CHIEF_ROLE]** agent whose domain is **[DOMAIN]**.

### Explicit Rules
- Search for how real organizations (human or agentic) structure the
  **[CHIEF_ROLE]** function's decision rights: what this role owns
  unilaterally, what needs escalation, and what is explicitly out of scope
  for it.
- Cross-check against this agent's existing boundary:
  - Owns: [OWNS]
  - Does NOT own: [DOES_NOT_OWN]
  - Reports to: [REPORTS_TO]
  Flag anywhere the research suggests this boundary is drawn differently in
  practice than PMCR-O currently has it — as a flag, not a recommendation.
- Look specifically for reasoning/planning strategies suited to this
  domain (e.g. a CFO-equivalent agent's numeric/risk reasoning needs differ
  from a CMO-equivalent agent's narrative/positioning reasoning) — tie any
  finding back to PMCR-O's existing 35-strategy O-Mode reasoning catalog if
  a match exists, and say explicitly if none of the 35 fit well.
- Same sourcing discipline as the MASTER prompt: production vs. prototype,
  no fabricated citations.

### Output Contract
1. This role's decision-rights boundary as commonly implemented elsewhere,
   compared point-by-point against this Chief's current Owns/Does-Not-Own.
2. Two or three reasoning-strategy patterns well-suited to this domain,
   each mapped to an existing O-Mode strategy id where one fits.
3. Sources.

### Boundary Reminder
Research input only, scoped to this one Chief — do not propose edits to
other Chiefs' boundaries, and do not propose PMCR-O code/file changes here.
