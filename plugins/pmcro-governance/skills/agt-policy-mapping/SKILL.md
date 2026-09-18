---
name: agt-policy-mapping
description: >
  Maps a real PMCR-O Earned Constraint / law (EC-004, EC-009, etc.) to an AGT
  policy.yaml rule, with tested Python examples and documented-syntax .NET
  examples. USE FOR: writing or reviewing a policy rule that encodes a PMCR-O
  law. DO NOT USE FOR: general AGT concepts (see agt-fundamentals) or wiring
  the kernel into the actual C# agents (see agt-dotnet-retrofit).
---

# AGT Policy Mapping — PMCR-O Laws as Policy Rules

Read `agt-fundamentals` first if you haven't — the condition-context shape
differs between Python and .NET, and that difference matters below.

## Worked example 1 — EC-004: Maker never self-scores

Law: only the Checker may issue PASS/LOOP/HALT; the Maker must never call a
self-scoring action.

**Policy rule (governance.toolkit/v1 — verified against the Python
`agentmesh.governance.govern()` engine, package `agent_governance_toolkit`
4.1.0):**

```yaml
apiVersion: governance.toolkit/v1
name: pmcro-laws
default_action: allow
rules:
  - name: ec-004-no-maker-self-score
    condition: "action.actor == 'maker' and action.type == 'self_score'"
    action: deny
    description: "EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT"
```

**Python call convention (tested, see verification note at bottom):**

```python
from agentmesh.governance import govern, GovernanceDenied

safe_action = govern(pmcro_action, policy="policy.yaml")
safe_action(action={"actor": "maker", "type": "self_score"})
# -> GovernanceDenied: EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT
```

**.NET equivalent (per the documented `PolicyEngine` condition syntax — write
against a flat, non-`action.`-prefixed context):**

```yaml
apiVersion: governance.toolkit/v1
name: pmcro-laws
default_action: allow
rules:
  - name: ec-004-no-maker-self-score
    condition: "agent_did == 'did:mesh:maker' and tool_name == 'self_score'"
    action: deny
    description: "EC-004: Maker never self-scores, only Checker PASS/LOOP/HALT"
```

```csharp
var result = kernel.EvaluateToolCall(
    agentId: "did:mesh:maker",
    toolName: "self_score"
);
// result.Allowed == false, result.Reason == "EC-004: ..."
```

## Worked example 2 — EC-009: MaxLoops 3, halt to human

Law: after 3 orchestrator loop iterations without a Checker PASS, halt and
escalate to a human rather than looping again.

`limit:` in the schema is a **rate** (`"100/hour"`), not a session-scoped
counter — it does not model "3 total, ever" cleanly. Model MaxLoops as an
explicit counter field compared in the condition instead:

```yaml
rules:
  - name: ec-009-max-loops
    condition: "action.type == 'orchestrator_loop' and action.loop_count > 3"
    action: deny
    description: "EC-009: MaxLoops 3, halt to human"
```

```python
for i in range(1, 6):
    safe_action(action={"type": "orchestrator_loop", "loop_count": i})
# iterations 1-3: allowed. iterations 4-5: GovernanceDenied.
```

The C# side already tracks a loop counter for this law (see the trail
frame's iteration count) — pass that value through as `args["loop_count"]`
on the `EvaluateToolCall` call and mirror the condition:
`condition: "tool_name == 'orchestrator_loop' and loop_count > 3"`.

## Template for mapping any other EC-xxx law

1. State the law as a single allow/deny sentence (as your `.pmcro/laws/`
   file already does).
2. Identify what a violation looks like as a tool call: which actor, which
   action type, which field crosses a threshold.
3. Write the Python rule first (nested under `action.`) since that's the
   engine this skill's examples were actually run against.
4. Translate to the .NET flat-context form for `PolicyEngine`.
5. Test both independently — do not assume a rule verified in one engine
   is verified in the other; see the schema-gap note in `agt-fundamentals`.

## Verification note

The Python examples above were executed against `agent_governance_toolkit`
4.1.0 (`pip install agent-governance-toolkit[full]`) with a standalone
`pmcro_action(action)` stand-in function — not against the real
`TrailService`/C# runtime. The `.NET` examples follow the documented
`PolicyEngine`/`GovernanceKernel` API from the official tutorial but have
not been executed in this repo yet — do that as part of
[[agt-dotnet-retrofit]] before relying on them in production.
