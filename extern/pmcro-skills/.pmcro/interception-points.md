# Interception points (MAF-native)

Microsoft's **Agent Hooks** contract — a framework-neutral governance layer — defines eight
interception points that bracket every agent's execution loop. Per Microsoft Agent Framework's own
architecture docs, Agent Hooks is not a separate layer: it "installs one middleware bundle across
the agent, chat, and function layers" that already exist in MAF's pipeline. `pmcro` adopts this
same three-layer structure so that any template-generated skill, plugin, or agent role is
observable and "teachable" the same way, regardless of what it does internally.

## The three real layers, and the eight named points on them

MAF's `Agent` pipeline has two composed parts (Python/C# docs use slightly different names for the
same shape):

1. **Agent middleware** — wraps the whole `run()` call. Contributes the outermost pair of points:
   **`agent_startup`** (before context/history resolution begins) and **`agent_shutdown`** (after
   the run fully completes), plus the data-content pair **`input`** (the request as it enters) and
   **`output`** (the response as it leaves) — lifecycle and content are named separately even
   though both sit at this one boundary.
2. **Chat middleware** — wraps each individual call to the underlying model, inside the
   `ChatClient`. Contributes **`pre_model_call`** / **`post_model_call`**. In a multi-turn
   tool-calling run this fires once per model turn, not once per run.
3. **Function middleware** — wraps each individual tool invocation, inside the `ChatClient`'s
   function-invocation loop. Contributes **`pre_tool_call`** / **`post_tool_call`**. Fires once per
   tool call, so three tool calls across a run means three pairs.

```
Agent middleware   [ agent_startup … input … ( chat + tool turns ) … output … agent_shutdown ]
  Chat middleware        [ pre_model_call … post_model_call ]   × once per model turn
  Function middleware    [ pre_tool_call … post_tool_call ]     × once per tool call
```

## Onion semantics — this is what makes a deny actually a deny

Each middleware layer wraps the next like a layer of an onion: it runs code *before* delegating
(to inspect or modify the input) and *after* the response comes back (to inspect or modify the
output) — and it can choose not to delegate at all, short-circuiting the pipeline by returning a
result directly. That short-circuit is what a guardrail *is*: "a deny at `pre_tool_call` means the
tool is not invoked," "a deny at `post_tool_call` means the result is discarded." A guard that only
logs and never has the power to short-circuit is not a guard, just telemetry — pmcro's Checker-only
law (below) has to be reconciled with this, since today a Checker only reports a verdict after the
fact rather than sitting in the pipeline as a live gate.

A deny can carry an **approval block** a human must resolve before the action proceeds. Agent Hooks
binds the approval to a hash of the exact content shown to the approver, so approving one action
never silently approves a changed one — approving an $840 refund does not cover an $8,400 one, even
if a guard's diff would otherwise look similar.

## Mapping onto a pmcro trail

A trail already logs one line per PMCR-O phase (`00-frame.jsonl`, `01-plan.jsonl`, `01-make.jsonl`,
`01-check.jsonl`, `01-reflect.jsonl`). The eight points sit one level finer, mostly inside the
Maker's own phase, and mirror the three layers above:

| Layer               | Points                              | Where in a trail                                                                                   |
|---------------------|--------------------------------------|-----------------------------------------------------------------------------------------------------|
| Agent middleware     | `agent_startup`, `input`             | folded into `00-frame.jsonl` (Law 1 — Log Before Act)                                               |
| Agent middleware     | `output`, `agent_shutdown`           | `01-check.jsonl` and `disposition.json` respectively                                                |
| Chat middleware      | `pre_model_call`, `post_model_call`  | optional sub-lines inside `01-make.jsonl`, one pair per model call the Maker makes                  |
| Function middleware  | `pre_tool_call`, `post_tool_call`    | optional sub-lines inside `01-make.jsonl`, one pair per tool/command run — the seam for a live approval gate, not just a logged one |

## Declaring it on a template

A template (see `templates/template.schema.md`) may add an optional `interception_points` array
naming which of the eight boundaries its generated skill or agent instruments. Omitting it means
"instrument all eight" for a `plugin` or `product` template, and just `pre_tool_call` /
`post_tool_call` for a narrow `skill` template that only wraps a couple of commands. This is what
makes a generated skill "teachable" on the platform: it exposes the same eight named seams no
matter what it was templated to do, so the platform can show one consistent view of any skill's
behavior instead of a bespoke one per skill.

## What's still open

This file gives the *vocabulary and layer mapping* MAF/Agent Hooks provide, not yet a working
implementation. Two specific gaps to resolve before trails actually enforce this, not just describe
it:

1. **Checker-only vs. live gating.** Law 3 (`.pmcro/laws.md`) says only the Checker issues a
   verdict, and today that happens *after* the Maker has already acted. A real `pre_tool_call` deny
   has to sit inside the Maker's own execution, before the tool runs — which means either the
   Checker needs a narrow, pre-registered set of live guards it can install into the Maker's
   function-middleware layer, or Law 3 needs an explicit carve-out for that case. Don't blur this
   without deciding it on purpose.
2. **Which runtime actually hosts the middleware.** MAF's layers are a library's call stack
   (`ChatClientAgent.RunAsync()` in-process); a pmcro trail today is files written by whatever
   agent or CLI is executing a phase, with no shared process. Wiring real middleware means picking
   where the Maker actually runs *as* a MAF `Agent` (likely `pmcro-code`, not the marketplace app),
   which is a `pmcro-code` implementation question, not a schema one.

Sources: [Agent Hooks: an open, framework-neutral AI governance contract](https://commandline.microsoft.com/agent-hooks-framework-neutral-ai-governance-contract/), [Agent pipeline architecture — Microsoft Learn](https://learn.microsoft.com/agent-framework/concepts/agents/agent-pipeline), [Adding Middleware — Microsoft Learn](https://learn.microsoft.com/agent-framework/journey/adding-middleware), [Agent Middleware concepts — Microsoft Learn](https://learn.microsoft.com/agent-framework/concepts/agents/middleware/).
