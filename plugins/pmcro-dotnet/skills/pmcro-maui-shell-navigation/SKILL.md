---
name: pmcro-maui-shell-navigation
description: Design and change .NET MAUI Shell navigation - tabs, flyouts, routes and parameters - without orphaning existing pages.
---

# pmcro-maui-shell-navigation

Use for `AppShell`, `TabBar`, `FlyoutItem`, `ShellContent`, route registration, and `GoToAsync` navigation.

## PMCR-O extension

Read the current `AppShell.xaml` and every `Routing.RegisterRoute` / `AddTransientWithShellRoute`
call before changing structure. A shell edit is a destructive edit: a page dropped from the visual
tree is unreachable even though it still compiles, and nothing fails to warn you.

Before the change, enumerate every reachable page. After the change, enumerate again and account
for each one. Any page that left the visual tree must either gain a registered route or be
deliberately retired, and the retirement must be named in the trail.

Validate:
- every pre-change page is still reachable, by tab or by registered route
- `//route` (absolute) vs `route` (push) is correct for each call site - absolute for tab switches,
  relative for detail pages that need a back stack
- code-behind that references named elements still resolves after the structural change
  (a `FlyoutFooter` control is still constructed when the flyout is disabled, so a name lookup
  in the constructor keeps working - confirm rather than assume)
- tab bar color properties are set from the theme's tokens, not literal values
- deep links and parameter-carrying routes still bind

## Evidence to record

The before/after page inventory, the route table, and the build output. A shell change that
compiles is not verified; the inventory is what makes it verifiable.
