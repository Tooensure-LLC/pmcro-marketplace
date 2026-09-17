---
name: pmcro-dotnet-project-analysis
description: Analyze a .NET repository before changing architecture, dependencies, or implementation.
---

# pmcro-dotnet-project-analysis

Build a factual map of the target before editing.

## Inspect
- solution and project graph
- target frameworks and SDK selection
- package references and central package management
- DI, configuration, hosting, and entry points
- source, tests, resources, generated code, and platform folders
- existing naming, namespace, and architectural boundaries

## Output
Report the relevant structure, dependencies, constraints, risks, and the smallest safe change surface. Preserve working conventions unless evidence justifies a change.

## Verification
After implementation, re-check the project graph and run the narrowest meaningful build/test set, then expand verification when risk warrants it.
