---
name: catalog-check
description: "PMCR-O catalog-check — part of pmcro-code plugin. USE FOR: catalog-check phase. DO NOT USE FOR: deciding O-Mode or dispatch."
---

# catalog-check

## Trigger On
- /pmcro-loop catalog-check [trail-id]
- /catalog-check

## Workflow — Laws Enforced
1. EC-SYS-003 Log Before Act: `await TrailService.OpenFrameAsync("catalog-check", intent)` BEFORE any file write
2. Read `agents/catalog-check/AGENT.md` if exists, else this SKILL.md is brain
3. Do work — Maker must cite evidence per EC-VERIFY-FIRST-001, no <placeholder> per PLAN-001
4. Write `.pmcro/trails/{type}/{uuid}/{NN}-catalog-check.jsonl` typed envelope
5. EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT
6. EC-009: MaxLoops 3, halt to human

## Where it writes
`.pmcro/trails/{source-type}/{uuid}/{NN}-catalog-check.jsonl`
