# CLAUDE.md

This file exists so Claude Code / Claude-based agents pick up repo context automatically.
The actual contract, laws, and current repository layout live in `AGENTS.md` at this
same root — read that file first, every session, before touching `src/`.

Two things worth restating here because they're easy to get wrong from habit:
- `src/api/` is the unified backend (Checker/Maker/Reflector/Orchestrator/Planner gRPC +
  REST hosts). `src/PMCR.Agents/` holds the separate MAF agent logic those hosts call into —
  don't confuse the two or merge them.
- `archive/` is out of scope for the build. If something there looks useful, port the
  specific piece deliberately into `src/api/` — don't re-wire the archived project back in.
