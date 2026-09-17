---
name: pmcro-maui-data-binding
description: Build compiled, source-generated .NET MAUI bindings that survive trimming and AOT.
---

# pmcro-maui-data-binding

Use for `BindingContext`, `x:DataType`, `DataTemplate`, `BindableLayout`, `CollectionView`
templates, converters, and `CommunityToolkit.Mvvm` observable state.

## PMCR-O extension

Check the project's XAML compilation mode first (`MauiXamlInflator`,
`MauiEnableXamlCBindingWithSourceCompilation`). Under source generation, an unresolved binding
path is a build-time failure, not a silent runtime null - which means the build log is real
evidence and should be captured as such.

Rules that hold regardless of the screen:

- every `DataTemplate` declares `x:DataType`; an untyped template falls back to reflection and
  loses the compile-time check that makes the build meaningful
- state lives on the model, presentation lives in XAML - but when a *state-to-visual* mapping is
  needed in code (a phase color, a role color), resolve it from the theme's resource dictionary
  rather than writing literal values into the model
- `[NotifyPropertyChangedFor]` on every derived property, or the UI silently keeps a stale value
- `[ObservableProperty]` on a field raises `MVVMTK0045` in WinRT targets; match whatever the
  project already does rather than introducing a second convention mid-codebase
- collection expressions assigned to `IReadOnlyList<T>` raise `CsWinRT1032` - declare the concrete
  array type instead

## Evidence to record

The build log, showing no new warnings introduced beyond the project's existing baseline. Compare
warning counts before and after; a rising count is a regression even when the build succeeds.
