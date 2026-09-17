# Template Driven Declarative Design

`pmcro` is not just a plugin marketplace — it's a reference implementation of a generic
**template-driven, declarative agent factory**. Tooensure (the company behind `pmcro`) builds
products by writing *declarations* of what should exist, then having an agent (the Maker, inside
a PMCR-O loop) materialize them — never by hand-writing scaffolding imperatively.

## The idea

A **template** is a declarative file that describes the shape of something to generate — a skill,
a plugin, an agent role, or a whole product — without prescribing the steps to build it. The agent
reads the template, fills in its declared variables, and produces files from it. Because generation
is agent-driven, it is bound by the same laws as everything else in this repo (`AGENTS.md`,
`docs/governance/laws.md`): a trail is opened before any file is written, the Maker cites evidence for what
it produced, and only a Checker can accept the result.

This is why `pmcro` (the marketplace app) and `pmcro-code` (the CLI) both make sense as products of
the *same* system: they are two templates run through the same template-driven pipeline, not two
independently hand-built codebases.

## Scope of what a template can produce

- **`skill`** — a `SKILL.md` (+ references), conforming to the open [Agent Skills
  specification](https://agentskills.io/specification): required YAML frontmatter (`name`,
  `description`), a lean body kept under roughly 500 lines / 5,000 tokens, and detail pushed into
  `references/` rather than inlined — exactly the shape `plugins/pmcro/skills/pmcro-loop/` already
  uses. A generated `SKILL.md`'s `description` carries the whole burden of getting the skill
  triggered at the right time (per the spec's progressive-disclosure model: an agent loads only
  `name`+`description` at startup, and reads the rest only once a task matches), so the Checker for
  a `skill` template should verify the description actually names when the skill is useful, not
  just that the file exists.
- **`plugin`** — a `plugin.json` bundling one or more skills, like `plugins/pmcro/plugin.json`.
- **`agent`** — an `*.agent.md` role definition, like `plugins/pmcro/agents/maker.agent.md`.
- **`product`** — a bundle of the above plus marketplace metadata: a whole installable
  product (e.g. a new company's plugin marketplace, scaffolded the same way `pmcro` itself was).
  A `product` template's `declares.files` lists the *same* four host-catalog paths this repo itself
  uses (`.claude-plugin/`, `.codex-plugin/`, `.cursor-plugin/`, `.agents/plugins/`, all pointing at
  one `body_ref`) — a new product gets multi-host marketplace support for free by following the
  convention, not by inventing its own.

See `template.schema.md` for the exact shape of a template file, and `examples/` for one worked
example of each kind above.

## Relationship to the laws

Generating from a template is a mutation like any other:

1. **Log Before Act** — the agent opens `.pmcro/trails/{uuid}/00-frame.jsonl` naming the template
   and its filled variables before writing any generated file.
2. **Verify First** — the Maker's `01-make.jsonl` cites the generated file paths as evidence, not a
   claim that generation "should have worked".
3. **Checker Only** — the Checker verifies the generated output actually matches the template's
   `declares.files` list and required variables before the trail can seal.
4. **MaxLoops** — a template that fails to generate correctly after 3 loops halts to a human rather
   than retrying indefinitely.
5. **Relative Paths Only** — a template's `declares.files` paths, and every path the Maker cites as
   evidence for them, must already be relative to the workspace root — a template is meant to be
   run again on someone else's machine, so nothing it generates or logs may assume this one.
