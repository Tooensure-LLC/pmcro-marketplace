---
name: agt-dotnet-retrofit
description: >
  Step-by-step retrofit of Microsoft.AgentGovernance's GovernanceKernel into
  PMCR-O's existing C# runtime and plugins/pmcro Claude Code plugin, without
  rewriting either. USE FOR: actually wiring governance into running PMCR-O
  code. DO NOT USE FOR: general AGT concepts (see agt-fundamentals) or writing
  policy rules from scratch (see agt-policy-mapping).
---

# Retrofitting AGT into PMCR-O (.NET)

Follows the official "Retrofit Governance onto an Existing Agent" pattern:
install, wrap, configure — no rewrite of existing logic. Read
`agt-fundamentals` and `agt-policy-mapping` first; this skill assumes you
already have a `policies/pmcro-laws.yaml` file from that skill.

## What's already there, and what it does NOT do

`plugins/pmcro/hooks/hooks.json` runs a Node.js `guard-log-before-act.js`
script on a `PreToolUse` matcher. This ONLY fires when Claude Code itself is
driving the session — it does not enforce anything for the real C# runtime
(`ProjectName.OrchestratorApi`, the gRPC agent services under
`src/api/`) running headless, outside Claude Code. That's the actual gap
this retrofit closes: the laws should hold whether a human is watching
through Claude Code or the runtime is executing unattended.

## Step 1 — Install

The package is already pinned in `Directory.Packages.props`. Confirm the
actual services reference it:

```
dotnet add src/api/ProjectName.OrchestratorApi/ProjectName.OrchestratorApi.csproj package Microsoft.AgentGovernance
```

If PMCR-O's C# agents are built on the real Microsoft Agent Framework
(`Microsoft.Agents.AI`), also add the MAF extension so governance hooks into
MAF's own execution pipeline instead of a hand-rolled call site:

```
dotnet add <project> package Microsoft.AgentGovernance.Extensions.Microsoft.Agents
```

## Step 2 — Wrap, don't rewrite

Wherever a PMCR-O phase (Orchestrator/Planner/Maker/Checker/Reflector)
currently calls `TrailService.OpenFrameAsync(...)` or equivalent, wrap the
*decision to act* — not the trail-writing itself — with a
`GovernanceKernel.EvaluateToolCall` check immediately before it:

```csharp
var result = _governance.EvaluateToolCall(
    agentId: $"did:mesh:{phase}",       // e.g. "did:mesh:maker"
    toolName: actionType,               // e.g. "self_score", "orchestrator_loop"
    args: new() { ["loop_count"] = loopCount }
);

if (!result.Allowed)
{
    // halt to human per the matched law, do not proceed to TrailService
    throw new GovernanceDeniedException(result.Reason);
}

await _trailService.OpenFrameAsync(...);   // existing call, unchanged
```

This is additive: the existing trail-writing code path is untouched, and a
denied call never reaches it.

## Step 3 — Configure once, inject everywhere

Register a single `GovernanceKernel` in DI (matches the AppHost/ServiceDefaults
pattern already used across `ProjectName.OrchestratorApi` and the agent gRPC
services):

```csharp
builder.Services.AddSingleton(new GovernanceKernel(new GovernanceOptions
{
    PolicyPaths = new() { "policies/pmcro-laws.yaml" },
    ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
    EnableAudit = true,
    EnableMetrics = true,   // emits System.Diagnostics.Metrics -> your existing OTLP exporter
}));
```

Because `EnableMetrics` uses `System.Diagnostics.Metrics`, it rides the same
OpenTelemetry pipeline `ProjectName.ServiceDefaults` already wires up — no
separate exporter needed.

## Step 4 — Verify before trusting

Do not assume a policy rule written in `agt-policy-mapping` works against
the real `PolicyEngine` until it's actually exercised:

```
dotnet test agent-governance-dotnet/AgentGovernance.sln   # upstream package tests
```

Then write one local test per PMCR-O law: call `EvaluateToolCall` with an
input that should be denied and one that should be allowed, assert
`result.Allowed` and `result.Reason` match the law's own wording. This is
the same shape as the Python proof in `agt-policy-mapping` — one test per
law, not a generic smoke test.

## Step 5 — Decide what happens to the Claude Code hook

Once the C# runtime enforces laws directly, `hooks.json`'s
`guard-log-before-act.js` becomes a redundant (but harmless) second check
specifically for Claude-Code-driven sessions — keep it as defense-in-depth
rather than removing it, since it catches the case of a human directly
issuing Write/Edit tool calls that never pass through the C# runtime at all.

## Open items (not yet done — flag before claiming this is complete)

- No `policies/pmcro-laws.yaml` exists in the repo yet — `agt-policy-mapping`
  gives the shape, but the full set of `.pmcro/laws/colony-laws.md` rules
  has not been transcribed into it.
- `GovernanceKernel` has not been registered in any real `Program.cs` yet —
  Steps 1-3 above are the plan, not a completed change.
- Step 4's per-law tests do not exist yet.
