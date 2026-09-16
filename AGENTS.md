# AGENTS.md — Root Contract

All agents MUST:
1. EC-SYS-003 Log Before Act — open .pmcro/trails/{type}/{uuid}/{NN}-{role}.jsonl BEFORE any file mutation.
2. EC-VERIFY-FIRST-001 — Maker must cite real evidence (command output, diffs).
3. EC-004 — Maker never self-scores. Only Checker issues PASS/LOOP/HALT.
4. EC-009 — MaxLoops=3, halt to human.
5. PLAN-001 — every frame is schema-valid JSONL, no `<placeholder>` left.

All five exist as real, callable guard clauses in `src/PMCR.Core/Laws/` — but as of 2026-09-16 none
of them are actually called by any phase agent. See `.pmcro/tool-reference.md` for the full,
honest status of every real capability in this repo (what's implemented, declared-but-unused, or a
stub) before assuming any of the above is enforced rather than just true in principle.

PMCR-O role prompts (the actual instruction source `dotagent.toml` loads) live at
`plugins/pmcro/skills/{orchestrator,planner,maker,checker,reflector}/SKILL.md` in this repo —
PMCR-O roles here are C# services guided by those skill files, not skill packages executed
directly. `extern/pmcro-skills/` is governance/reference content (the laws' original markdown form,
MAF-alignment notes, declarative templates) folded in from a sibling repo — read it for context and
conventions, not as the live prompt source.
