# pmcro

Minimal PMCR-O loop. One skill. Five laws. Trails as ground truth.

**Not the canonical product.** [`Tooensure-LLC/pmcro-marketplace`](https://github.com/Tooensure-LLC/pmcro-marketplace)
is the real product repo (a separate, real .NET solution — Domain/Application/Infrastructure, real
C# laws, an Aspire host — already under active local development). This tree has been folded into
it at `extern/pmcro-skills/` as reference/governance content; it is not itself pushed anywhere, and
`Tooensure-LLC/pmcro-marketplace` is not this repo's own remote. Related repo:
[`pmcro-runtime`](https://github.com/ShawnDelaineBellazanLoop/pmcro-runtime), a separate .NET execution engine.
(Note: `ShawnDelaineBellazanLoop/pmcro-skills` also exists on GitHub, but is an unrelated Python project —
don't confuse it with this tree.)

```text
Seed → True Intent
        ↓
Orchestrator
        ↓
Planner → Maker → Checker → Reflector
        ↓
Trail sealed
```

## Install by host

### Claude Code
```bash
/plugin marketplace add <you>/pmcro
/plugin install pmcro@pmcro
```
Catalog: `.claude-plugin/marketplace.json` → `plugins/pmcro`.

### Cline (validated 2026)
Skills are **not** Claude-style marketplaces. Use Agent Skills paths:

| Scope | Path |
|-------|------|
| Project | `.cline/skills/pmcro-loop/` |
| Global | `~/.cline/skills/pmcro-loop/` |

```bash
# Option A — already in this repo (project skill auto-discovered)
# Enable: Cline Settings → Features → Enable Skills

# Option B — install from git via skills CLI
npx skills add <you>/pmcro
# or
cline skill install <you>/pmcro --skill pmcro-loop
```

Project rules (always on): `.clinerules`  
Skill (on-demand): `.cline/skills/pmcro-loop/SKILL.md`

### Codex / Cursor / agents
```text
.codex-plugin/marketplace.json
.cursor-plugin/marketplace.json
.agents/plugins/marketplace.json
```
Same Claude-compatible catalog shape → `plugins/pmcro`.

## Run a cycle

Load `pmcro-loop` and follow it.  
Every mutation opens a trail under `.pmcro/trails/{uuid}/` first.

## Laws

See `.pmcro/laws.md` and `AGENTS.md`.
