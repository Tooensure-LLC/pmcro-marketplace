---
name: figma-make
description: Govern Figma Make work against a real codebase: configure .figma/make, preserve Git boundaries, connect UI intent to PMCR-O execution, and verify the result with evidence.
---

# PMCR-O Figma Make

Use this skill when Figma Make Beta is being used to design, implement, or refine the PMCR-O UI in a real repository.

## Core rule

Figma Make is a coding surface, not the source of truth for PMCR-O governance. The target repository remains authoritative. Treat every generated UI change as a PMCR-O Maker action: inspect first, plan the smallest change, capture evidence, verify, then seal the trail.

## Before changing code

1. Identify the actual target repository and branch.
2. Read `AGENTS.md`, `CLAUDE.md`, and existing PMCR-O laws when present.
3. Inspect the app entry point, package manager, scripts, and existing UI/design-system conventions.
4. Determine the real development command, health endpoint, and port. Never assume Vite/Next/React or port 3000.
5. Open a PMCR-O trail before mutation when the target repo is governed by PMCR-O.

## Figma Make repository wiring

Figma's current local-codebase setup uses a root `.figma/make/` directory containing exactly five operational files:

- `setup` — one-time system dependency setup; idempotent.
- `install` — project dependency installation; runs each session.
- `dev` — starts the development server.
- `verify` — waits for the server and returns non-zero on failure.
- `env` — environment file; `PORT` and `FIGMA_MAKE_URL` are mandatory.

These files are intended to be committed to the repository's main branch so new Make sessions inherit the configuration. Do not add `.figma/make/` to `.gitignore`.

## Configuration procedure

When configuring a target repository:

1. Create `.figma/make/` at repository root.
2. Adapt the five template files in this plugin to the repository's actual toolchain.
3. Keep scripts idempotent and exit `0` on success.
4. Use a real readiness endpoint in `verify`; do not use a fake health check.
5. Set `PORT` and `FIGMA_MAKE_URL` to the actual dev server.
6. Commit the configuration on the main branch before expecting other Make users to inherit it.
7. Keep secrets out of `env`; use the repository's approved secret mechanism.

## Git boundaries

Figma Make can create local commits, branches, and pull requests. Preserve normal repository review boundaries:

- Never commit secrets, tokens, credentials, local caches, or machine-specific state.
- Do not ignore `.figma/make/` merely because it was generated; Figma explicitly expects these configuration files to be committed.
- Keep generated dependency directories such as `node_modules/` governed by the existing repository `.gitignore`.
- If Make proposes unrelated generated files, inspect them before accepting them.
- Prefer a dedicated branch for UI work when the repository's workflow requires it.

## PMCR-O UI/UX loop

For a UI request, translate messy seed intent into a concrete UI outcome before editing. Then:

1. **Planner:** define the screen/state, acceptance criteria, affected components, and API/data dependencies.
2. **Maker:** implement the smallest coherent UI change and preserve existing design-system primitives.
3. **Checker:** run the real build/test/verification commands and inspect the rendered result. Only Checker issues PASS, LOOP, or HALT.
4. **Reflector:** record durable lessons only after an ACCEPT disposition.

Never claim visual success from source code alone. Evidence should include the actual dev-server/build result and, when available, a rendered screenshot or Figma inspection result.

## Figma MCP / connectors

Figma Make can consume external MCP connectors. A connector is configured in Figma's UI and requires a reachable MCP server URL; do not invent a localhost/public endpoint. The PMCR-O plugin may document the connector contract, but credentials and connector permissions remain outside the repository.

For Claude Code, Figma's official Claude Code plugin is the preferred way to add the Figma MCP server and Figma workflow skills. Do not duplicate the official server configuration unless a concrete PMCR-O-specific server is required.

## References

Read `references/figma-make-repository.md` for the current Figma Make Beta repository contract before changing `.figma/make` files.
