# PMCR-O Laws

## EC-SYS-003 — Log Before Act
Open a trail frame before any state-mutating write.
Path: `.pmcro/trails/{uuid}/{NN}-{role}.jsonl`

## EC-VERIFY-FIRST-001 — Verify First
Maker must attach real evidence (stdout, diff, file exists).
No placeholders. No “should work”.

## EC-004 — Checker Verdict Only
Maker does not score its own work.
Only Checker emits PASS | LOOP | HALT.

## EC-009 — MaxLoops
Max 3 loops on the same trail. Then HALT to human.
No infinite retry.

## EC-PORTABLE-005 — Relative Paths Only (Trail as Product)
Every file path recorded inside a trail (`.jsonl` frames, `disposition.json`) must be relative to
the workspace root. Absolute host paths (`/home/user/...`, `C:\Users\...`) are forbidden: a trail
is a portable product — shareable, replayable on another machine, usable as fine-tuning data —
and an absolute path breaks all three.

## LAW-010 — Frames Are Append-Only
A trail frame, once written, is never updated or deleted. A correction is a new frame that
references the prior one. This ID is adopted directly from `pmcro-runtime`'s
`.pmcro/policies/resource-operations.json` rather than given a new `EC-` code — see
"Reconciliation with pmcro-runtime" below.

## Reconciliation with pmcro-runtime

`pmcro-runtime` (the .NET execution engine) governs its own resources with law/architecture IDs —
`LAW-002` (evidence immutability), `LAW-010` (append-only frames/events, adopted above), and
`ARCH-013` (schema/contract stability) — that don't use this file's `EC-` prefix. Its
`resource-operations.json` (mirrored at `.pmcro/policies/resource-operations.json` in this repo)
states it is meant to be *the same file* both a Python control plane here and the .NET runtime
there read — but as of this writing, `pmcro-runtime` only forward-references those three IDs in
`resource-operations.json`'s reason strings; it has no file anywhere that defines the full
`LAW-xxx`/`ARCH-xxx` registry. Renumbering `EC-SYS-003`, `EC-VERIFY-FIRST-001`, `EC-004`,
`EC-009`, and `EC-PORTABLE-005` onto that scheme would mean guessing IDs that don't exist yet —
don't do that. `LAW-002` (evidence must not be revisable) overlaps in spirit with
`EC-VERIFY-FIRST-001` (evidence must be real) but is stricter and distinct; treat them as two
laws until `pmcro-runtime` publishes its actual registry to reconcile against.
