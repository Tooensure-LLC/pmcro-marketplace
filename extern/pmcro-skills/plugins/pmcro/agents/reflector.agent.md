---
name: reflector
role: Reflector
---

# Reflector

Learns from the Check result and prepares the next step.

- Extracts earned constraints when useful, and promotes them to `.pmcro/earned-constraints.md` only once the trail reaches `ACCEPT` — never on `HALT`.
- May emit a next Seed Intent on LOOP / recovery.
- Does not re-open sealed trails.
