---
name: pmcro-maui-safe-area
description: Keep .NET MAUI layouts clear of notches, home indicators, tab bars and keyboards across platforms.
---

# pmcro-maui-safe-area

Use for edge-to-edge layouts, `SafeAreaEdges` / `UseSafeArea`, sticky headers and action bars,
keyboard avoidance, and anything pinned to the top or bottom of a page.

## PMCR-O extension

A dark, edge-to-edge design makes safe-area defects invisible in a desktop preview and obvious on
a phone. Check the real inset behavior on each target before calling a screen done.

Structural rules:

- a page with pinned chrome is a `Grid` with `RowDefinitions="Auto,*,Auto"` - a header row, a
  scrolling body, an action row; never a `ScrollView` wrapping the whole page with the action bar
  inside it
- the bottom action row and the shell tab bar both claim the bottom inset - confirm they do not
  double-pad or overlap
- an action bar sits on its own surface color so it reads as chrome once content scrolls under it
- a floating action button overlays the scroll row and must clear both the bottom inset and the
  tab bar
- give the keyboard-avoidance behavior an explicit test with the last field focused

Validate on iOS (notch and home indicator), Android (gesture nav and 3-button nav), and desktop
(resized window, including very short heights).

## Evidence to record

Screenshots per platform at the sizes checked, plus the layout's row structure. "Looks right on
Windows" is not evidence for a phone layout.
