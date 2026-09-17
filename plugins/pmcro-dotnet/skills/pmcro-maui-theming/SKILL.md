---
name: pmcro-maui-theming
description: Build and validate maintainable .NET MAUI themes using project-native resources and current platform behavior.
---

# pmcro-maui-theming

Use for colors, typography, spacing, light/dark themes, resource dictionaries, and visual tokens.

## PMCR-O extension
Inspect the existing resource architecture before adding resources. Prefer `ResourceDictionary`, `DynamicResource`, and `AppThemeBinding` where appropriate to the current MAUI version.

Validate:
- light/dark and platform behavior
- accessibility and contrast
- shared token naming and ownership
- startup/resource lookup behavior
- visual consistency across affected screens

For Figma-driven work, map design tokens to MAUI resources rather than copying screen-specific values. Record design and implementation evidence in the trail.
