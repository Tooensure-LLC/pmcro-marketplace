# Figma Make repository contract

Verified against Figma Learn documentation on 2026-09-16.

## Local-codebase Beta

Figma Make's local-codebase workflow is currently a closed beta and uses the Figma Beta desktop app for Mac. It connects to a real Git repository and can work with local code, branches, commits, and pull requests.

## Required repository configuration

Create this directory at the repository root:

```text
.figma/
└── make/
    ├── setup
    ├── install
    ├── dev
    ├── verify
    └── env
```

`setup`, `install`, `dev`, and `verify` are shell scripts. They must be idempotent and return exit code `0` on success. `env` uses env-file syntax.

Required `env` keys:

```text
PORT=<actual-dev-server-port>
FIGMA_MAKE_URL=http://localhost:<actual-dev-server-port>
```

`BOOTSTRAP_TIMEOUT` is optional.

## Commit policy

Figma recommends committing `.figma/make/` to the main branch. Every branch created from main then inherits the setup, so the configuration should not be ignored.

## GitHub

GitHub is natively supported by the Figma Beta desktop workflow. Figma Make can clone repositories, push branches, and create pull requests during the current closed beta. The Figma GitHub app must be installed for the organization/team as required by the account setup.

## MCP distinction

Figma Make is an MCP client when it uses connectors. The Figma MCP server is different: it exposes Figma file data to other MCP clients. Custom connectors require a reachable MCP server URL and appropriate authentication. Write tools are disabled by default and must be explicitly enabled.

For Claude Code, Figma currently recommends its official Claude Code plugin for the remote Figma MCP server and related Agent Skills. PMCR-O should add its own governance/UX workflow rather than duplicating that server.

## PMCR-O interpretation

`.figma/make/` is infrastructure configuration, not disposable generated output. The PMCR-O trail remains the audit source for governed changes. Record the actual commands, diffs, verification output, and rendered UI evidence.
