---
name: dependency-resolver
description: "PMCR-O dependency-resolver — part of pmcro plugin. USE FOR: dependency-resolver phase. DO NOT USE FOR: deciding O-Mode or dispatch."
---

# dependency-resolver

## Trigger On
- /pmcro-loop dependency-resolver [trail-id]
- /dependency-resolver

## Workflow — Laws Enforced
1. EC-SYS-003 Log Before Act: `await TrailService.OpenFrameAsync("dependency-resolver", intent)` BEFORE any file write
2. Read `agents/dependency-resolver/AGENT.md` if exists, else this SKILL.md is brain
3. Do work — Maker must cite evidence per EC-VERIFY-FIRST-001, no <placeholder> per PLAN-001
4. Write `.pmcro/trails/{type}/{uuid}/{NN}-dependency-resolver.jsonl` typed envelope
5. EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT
6. EC-009: MaxLoops 3, halt to human

## Where it writes
`.pmcro/trails/{source-type}/{uuid}/{NN}-dependency-resolver.jsonl`
