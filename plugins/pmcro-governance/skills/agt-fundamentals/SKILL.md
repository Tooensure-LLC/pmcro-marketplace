---
name: agt-fundamentals
description: >
  Explains what the Microsoft Agent Governance Toolkit (AGT) is and how its
  core primitives work, before any policy is written or any code is wired up.
  USE FOR: understanding govern()/GovernanceKernel, policy YAML shape,
  condition expressions, and the .NET-vs-Python/TS/Rust schema gap. DO NOT
  USE FOR: writing an actual policy rule (see agt-policy-mapping) or wiring
  AGT into PMCR-O's agents (see agt-dotnet-retrofit).
---

# AGT Fundamentals

AGT is Microsoft's polyglot agent-governance layer (public preview). It is not
a portal, a service, or something you deploy separately — it's a library that
sits between an agent and the tools it calls, and evaluates a YAML policy on
every call.

```
Your Agent → [AGT] → Tool / API / Database
              policy check: allow / deny / warn / require_approval
              audit log: who did what, when
```

## Language SDKs

| Language   | Package                              |
|------------|---------------------------------------|
| Python     | `agent-governance-toolkit[full]`      |
| TypeScript | `@microsoft/agentmesh-sdk`            |
| .NET       | `Microsoft.AgentGovernance`           |

PMCR-O's C# runtime already pins `Microsoft.AgentGovernance` in
`Directory.Packages.props`. Nothing else is required to start using it.

## The core primitive, per language

**Python** (2-line quickstart):

```python
from agentmesh.governance import govern

safe_tool = govern(my_tool, policy="policy.yaml")
safe_tool(action={"actor": "maker", "type": "self_score"})  # raises GovernanceDenied if blocked
```

Note the calling convention: pass an `action=` kwarg whose value is a dict.
`GovernedCallable._build_context` only nests fields under `context["action"]`
when they arrive that way — other top-level kwargs are wrapped individually
as `{key: {"value": val}}`, which will silently NOT match a condition like
`action.actor == 'maker'`. Always pass the fields your policy conditions
reference inside the `action={...}` dict.

**.NET** (GovernanceKernel):

```csharp
var kernel = new GovernanceKernel(new GovernanceOptions
{
    PolicyPaths = new() { "policies/default.yaml" },
    ConflictStrategy = ConflictResolutionStrategy.DenyOverrides,
});

var result = kernel.EvaluateToolCall(
    agentId: "did:mesh:orchestrator",
    toolName: "orchestrator_loop",
    args: new() { ["loop_count"] = 4 }
);

if (!result.Allowed) { /* handle denial */ }
```

.NET's context is built differently from Python's: `agentId` and `toolName`
become top-level `agent_did` / `tool_name` fields, and everything in `args`
is merged in flat — there is no `action.` prefix. A condition written for
Python (`action.actor == 'maker'`) will not evaluate the same way against
.NET's context; write `tool_name == 'self_score'` instead. See
`agt-policy-mapping` for side-by-side, tested examples of both.

## Known gap: policy schema divergence (as of this AGT public-preview release)

The official .NET tutorial states this explicitly, not something inferred:

> The Python, Rust, and TypeScript runtimes evaluate ACS v5 manifests. The
> .NET `PolicyEngine` still uses the `governance.toolkit/v1` document... Do
> not use this shape with the other runtimes.

In practice this means: a policy file written for .NET's `PolicyEngine`
(`apiVersion: governance.toolkit/v1`) is NOT guaranteed to parse or evaluate
identically under the current Python `agent-governance-toolkit-core` engine
(which targets ACS v5), even though both accept YAML with `rules:`/
`condition:`/`action:` keys. The older `agentmesh.governance.govern()` Python
entry point (package `agent_governance_toolkit`, deprecated in favor of
`agent-governance-toolkit-core`) DOES still speak `governance.toolkit/v1` —
that's what was used to validate the examples in `agt-policy-mapping`. Before
building anything cross-language on this, check
https://microsoft.github.io/agent-governance-toolkit/tutorials/55-agent-control-specification/
for the current state of .NET's ACS v5 migration — this is a public-preview
project and the gap may have closed since this skill was written.

## Condition expression syntax (shared across engines, context shape differs)

```
tool_name == 'file_write'          # equality
agent_did != 'did:mesh:admin'      # inequality
risk_score > 0.8                   # numeric comparison
tool_name in blocked_tools         # list membership
data.contains_pii                  # nested dot-path, truthiness
a and b / a or b                   # compound
```

## Four moving parts, every time

1. **Configure** — create a kernel/`govern()` wrapper with a conflict strategy
   (`deny_overrides` is the safe default — any deny wins).
2. **Load** — one or more policy YAML files (or an inline string).
3. **Evaluate** — every tool call goes through the engine before it runs.
4. **Act** — check `result.Allowed` / catch `GovernanceDenied`; never skip
   this step, since the call already executed side-effect-free evaluation
   but has NOT executed the tool itself yet at that point.

Next: [[agt-policy-mapping]] to write PMCR-O's actual laws as policy rules.
