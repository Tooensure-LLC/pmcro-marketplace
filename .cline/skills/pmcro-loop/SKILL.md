---
name: pmcro-loop
description: Run one PMCR-O cycle — open trail, plan, make, check, reflect, seal.
---

# pmcro-loop

## When to use
Any task that needs governed execution with an auditable trail.

## Cycle

1. **Open trail**  
   Create `.pmcro/trails/{uuid}/00-frame.jsonl` with seed + true intent.  
   (Log Before Act — before any mutation.)

2. **Plan**  
   Act as Planner. Read `.pmcro/earned-constraints.md` first. Write `01-plan.jsonl`. Minimum plan + acceptance criteria.

3. **Make**  
   Act as Maker. Execute plan. Write `01-make.jsonl` with artifacts **and evidence**.

4. **Check**  
   Act as Checker. Verify against plan. Write `01-check.jsonl` with PASS | LOOP | HALT.

5. **Reflect / Seal**  
   - PASS → write `disposition.json` = ACCEPT, stop. If any loop happened on this trail, Reflector promotes the durable lesson to `.pmcro/earned-constraints.md`.  
   - LOOP and loops < 3 → Reflector notes learning, next cycle on same trail.  
   - HALT or loops ≥ 3 → disposition = HALT, stop for human.

## Laws
See `.pmcro/laws.md` and `AGENTS.md`. Non-negotiable.
