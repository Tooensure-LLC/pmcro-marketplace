# Earned Constraints

Rules the Reflector has promoted from a real `LOOP` — each one exists because a Checker rejected
a Maker's attempt for a concrete reason, and the Reflector judged the lesson worth keeping past
this one trail. Read this file at the start of every cycle, alongside `.pmcro/laws.md`, so the
Planner and Maker inherit every mistake this workspace has already made and paid for.

## How an entry gets here

1. Checker issues `LOOP` on a trail.
2. Reflector inspects what the Maker did and what the Checker rejected, and judges whether the
   failure reveals a durable rule — not a one-off slip — per `plugins/pmcro/agents/reflector.agent.md`.
3. Only once that same trail later reaches `disposition: ACCEPT` does the Reflector append the
   constraint here, one line per constraint, naming the trail it was earned on. A trail that ends
   in `HALT` promotes nothing — an unresolved failure is not yet a lesson.

## Format

`- {constraint, stated as a rule the Maker must follow} (earned: trail {uuid})`

## Entries

(none yet)
