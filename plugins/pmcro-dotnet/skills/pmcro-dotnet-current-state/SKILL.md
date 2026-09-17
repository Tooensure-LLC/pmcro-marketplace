---
name: pmcro-dotnet-current-state
description: Validate current .NET and MAUI APIs, packages, tooling, and guidance before version-sensitive decisions.
screen: reference
---

# pmcro-dotnet-current-state

Use before relying on framework behavior that may change across releases.

## Procedure
1. Identify the exact target framework, SDK, workload, package, and platform.
2. Check the repository's `global.json`, project files, package management, and lock/version constraints.
3. Research authoritative Microsoft/.NET sources for the current supported behavior.
4. Prefer released documentation and release notes; clearly separate preview guidance.
5. Compare the researched state with the local implementation.
6. Record source, date checked, relevant version, and conclusion in the PMCR-O trail.

## Gate
If authoritative evidence is missing or contradictory, do not silently guess. Mark the decision unresolved and route it through Checker/HALT as appropriate.
