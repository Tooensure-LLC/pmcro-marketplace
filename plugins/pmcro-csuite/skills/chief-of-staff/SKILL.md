---
name: chief-of-staff
description: "PMCR-O chief-of-staff — part of pmcro-csuite plugin. USE FOR: chief-of-staff phase. DO NOT USE FOR: deciding O-Mode or dispatch."
---

# chief-of-staff

## Trigger On
- /pmcro-loop chief-of-staff [trail-id]
- /chief-of-staff

## Workflow — Laws Enforced
1. EC-SYS-003 Log Before Act: `await TrailService.OpenFrameAsync("chief-of-staff", intent)` BEFORE any file write
2. Read `agents/chief-of-staff/AGENT.md` if exists, else this SKILL.md is brain
3. Do work — Maker must cite evidence per EC-VERIFY-FIRST-001, no <placeholder> per PLAN-001
4. Write `.pmcro/trails/{type}/{uuid}/{NN}-chief-of-staff.jsonl` typed envelope
5. EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT
6. EC-009: MaxLoops 3, halt to human

## Where it writes
`.pmcro/trails/{source-type}/{uuid}/{NN}-chief-of-staff.jsonl`
