---
name: prompt-engineer
description: >
  Draft, review, or improve any prompt, system instruction, agent persona
  file, or SKILL.md body used anywhere in the PMCR-O ecosystem. Make sure to
  use this whenever the user asks to "write a prompt," "reword" a message,
  "make a skill/agent better at X," design a system instruction for a Chief
  persona, or when a new skill is being authored (by create-skill or
  scaffold-skill) and needs its instructional body written well — even if
  the user just says "make this clearer" about any text meant to steer an
  LLM. This is the shared source other skill-authoring tools should call
  into rather than each inventing their own prompt-writing conventions.
---

# Prompt Engineer

This skill holds PMCR-O's actual prompt-crafting knowledge — not a new
convention, but Anthropic's own published techniques, applied consistently.
Other tools (`create-skill`, `pmcro-marketplace-directory:scaffold-skill`)
handle the mechanical scaffolding of a skill package; this skill is what
they should draw on for the words that go inside it.

## Decision tree — which technique(s) does this prompt need?

1. **Is the output inconsistent or off-target?** → Clarity & Directness first,
   always. Most bad outputs trace back to an ambiguous ask, not a missing
   advanced technique. See "Clarity & Directness" below.
2. **Is the format/style wrong but the content is right?** → Multishot
   examples. Show 2-3 examples of the exact input→output shape wanted.
3. **Is the reasoning shallow or does it skip steps on a hard problem?** →
   Chain-of-thought. Ask for step-by-step thinking before the answer.
4. **Is the prompt long, multi-part, or does the model conflate sections?**
   → XML tags to separate instructions/context/examples/output.
5. **Does the model need a stable persona or expertise framing across many
   calls?** → Role prompting (system prompt), not repeated per-message
   reminders.
6. **Is the output almost right but needs a fixed prefix/format lock-in?**
   → Prefilling the start of the response.
7. **Is this one prompt trying to do 3+ unrelated things?** → Split into a
   prompt chain — one prompt per subtask, feeding output to the next.
8. **Is this a PMCR-O-specific instructional file (SKILL.md, agent.md,
   Chief persona)?** → Apply 1-7 as needed, then also apply the house style
   in `references/sdlc-house-style.md` — don't freelance a new structure
   when this repo already has one.

Techniques compose — a good complex prompt often uses several at once.

## 1. Clarity & Directness

State the task, context, and desired outcome explicitly. Don't make the
model infer what "good" means.

- Bad: "Write something about the CFO agent's boundaries."
- Better: "Write the Owns/Does-Not-Own section for the CFO agent's
  AGENT.md. Owns: budget approval under $10k, financial reporting cadence.
  Does not own: hiring decisions (CHRO), technical architecture (CTO). One
  sentence each, imperative voice, matching the CEO agent's existing
  format."

Golden rule: show the draft prompt to a colleague with no context — if they
can't follow it and produce the output themselves, the model can't either.

## 2. Multishot examples

2-5 diverse, relevant examples beat a paragraph of abstract description.
Wrap each in `<example>` tags so the model can tell where one ends and the
next begins. Cover edge cases the examples should also teach, not just the
happy path.

## 3. Chain-of-thought (let it think first)

For anything requiring real reasoning (a Checker verdict, a Planner
decomposition, a Reflector diagnosis), ask for thinking before the answer:
"Think step-by-step in `<thinking>` tags, then give your final answer in
`<answer>` tags." Don't ask for chain-of-thought on simple lookups — it
adds latency for no quality gain there.

## 4. XML tags for structure

Use tags to separate distinct parts of a prompt so the model doesn't
conflate instructions with the content they operate on:

```xml
<instructions>...</instructions>
<context>...</context>
<examples>...</examples>
<output_format>...</output_format>
```

Consistent tag names across a prompt matter more than which names you pick
— reuse the same tag for the same kind of content every time.

## 5. Role prompting (system prompts)

Give the model a role in the system prompt, not repeated in every user
turn: "You are the Checker phase of a PMCR-O cycle. You verify Maker output
against Planner's success criteria and issue exactly one of PASS/LOOP/HALT
— never a partial score." This is more durable and token-efficient than
re-stating context each message, and matches how PMCR-O's own
`agents/*.agent.md` files already work — check those before writing a new
persona from scratch.

## 6. Prefilling

Start the model's response for it to lock in format: prefill `{` to force
JSON output, or `## Analysis\n` to force a heading structure. Useful for
skipping preamble and enforcing structure, not for making the model say
something it wouldn't otherwise conclude.

## 7. Prompt chaining

If a task has 3+ genuinely independent subtasks (research → draft → check),
chain separate prompts rather than one mega-prompt — matches PMCR-O's own
Plan→Make→Check→Reflect decomposition, which is itself a prompt chain with
a trail contract between links. Each link's output becomes the next link's
input; don't smuggle unrelated instructions into one link because it's
convenient.

## House style for PMCR-O instructional files

See `references/sdlc-house-style.md` before writing any new SKILL.md body,
AGENT.md persona file, or Chief boundary definition — it documents the
existing Role Framing / Explicit Rules / Output Contract / Boundary
Reminder convention already used in `SDLC_PROMPTS.md`, so new files stay
consistent with what's already in the repo instead of drifting into a new
shape each time.

## What this skill does NOT do

It does not scaffold the file/folder structure of a new skill (that's
`create-skill` / `scaffold-skill`), and it does not decide whether a skill
should exist at all (that's a product decision for the user). It only makes
the words inside an instructional file better.
