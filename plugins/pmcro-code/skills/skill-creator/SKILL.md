---
name: skill-creator
description: "PMCR-O skill-creator — part of pmcro-code plugin. USE FOR: skill-creator phase. DO NOT USE FOR: deciding O-Mode or dispatch."
---

# skill-creator

## Trigger On
- /pmcro-loop skill-creator [trail-id]
- /skill-creator

## Workflow — Laws Enforced
1. EC-SYS-003 Log Before Act: `await TrailService.OpenFrameAsync("skill-creator", intent)` BEFORE any file write
2. Read `agents/skill-creator/AGENT.md` if exists, else this SKILL.md is brain
3. Do work — Maker must cite evidence per EC-VERIFY-FIRST-001, no <placeholder> per PLAN-001
4. Write `.pmcro/trails/{type}/{uuid}/{NN}-skill-creator.jsonl` typed envelope
5. EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT
6. EC-009: MaxLoops 3, halt to human

## Where it writes
`.pmcro/trails/{source-type}/{uuid}/{NN}-skill-creator.jsonl`
