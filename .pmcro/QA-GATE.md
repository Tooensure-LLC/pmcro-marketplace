# PMCRO MAUI QA Gate

## Mandatory capability
Use `pmcro-maui-qa` from `T:\pmcro-skills\.agents\skills\pmcro-maui-qa\SKILL.md` before accepting Marketplace UI/template work.

## Acceptance rule
Compilation alone is insufficient. QG-01 through QG-07 must be evaluated; runtime-dependent gates require actual device/emulator evidence.

## Required loop
Planner -> Maker -> Checker -> Reflector -> Orchestrator.

## Current disposition
The 2026-09-17 activation is `RETRY` / `NOT_ACCEPTED`. QG-07 fails because the generated template references `src\ProjectName.MauiServiceDefaults` outside its template root. Runtime visual, interaction, and accessibility gates remain unverified because `adb` is unavailable on the connected workstation.

## Re-entry condition
Do not seal the Marketplace template as accepted until the portability defect is corrected and QG-02/QG-04/QG-05/QG-06 have runtime evidence.
