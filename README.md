# pmcro-marketplace — Tooensure-LLC — PROD MIGRATED TO plugins/

> MIGRATION NOTE (2026): `catalog/` -> `plugins/` per Anthropic marketplace standard.

## Structure — NEW
```
Tooensure-LLC/pmcro-marketplace/
├── .claude-plugin/marketplace.json      # authority — points to plugins/
├── plugins/
│   ├── pmcro/skills/orchestrator,planner,maker,checker,reflector/
│   ├── pmcro-loop/skills/pmcro-loop/
│   ├── pmcro-csuite/skills/ceo,cto,coo,cfo,cro,cmo,clo,chro,domain-specialist/
│   └── pmcro-code/skills/git,filesystem,catalog-check,skill-creator,meta-runtime/
├── src/PMCR.Core/Laws/                  # EC-SYS-003, EC-VERIFY-FIRST-001, EC-004, EC-009, PLAN-001
│   ├── EC_SYS_003_LogBeforeAct.cs
│   ├── EC_VERIFY_FIRST_001.cs
│   ├── EC_004_CheckerOnly.cs
│   └── EC_009_MaxLoops.cs
├── src/Domain/Common/BaseEntity.cs     # Id = Guid.NewGuid()
├── src/Application/IGenericRepository<T> where T:BaseEntity
├── src/Application/IUnitOfWork.cs      # Repository<T>() + GetRepository<TRepo>() CLOSED
├── AppHost/Program.cs                  # Aspire: postgres + ollama + 5 agents
└── .pmcro/config.json
```

## Install — NEW
```bash
/plugin marketplace add Tooensure-LLC/pmcro-marketplace
/plugin install pmcro@pmcro-marketplace
/plugin install pmcro-loop@pmcro-marketplace
/plugin install pmcro-csuite@pmcro-marketplace
```

Old `catalog/` is shim only.

## Laws
- EC-SYS-003 Log Before Act
- EC-VERIFY-FIRST-001 Verify First
- EC-004 Checker Only
- EC-009 MaxLoops 3
- PLAN-001 No placeholder

## Tool Reference

`.pmcro/tool-reference.md` — every real capability (trail primitives, laws, OMode, phase agents,
Ollama runtime, declared MCP servers) with an honest status: Implemented, Declared but unused, or
Stub. Read it before assuming a law is enforced or an agent does real work — as of 2026-09-16, the
laws are callable but uncalled, and every phase agent's "do work" step is a literal placeholder
comment.
