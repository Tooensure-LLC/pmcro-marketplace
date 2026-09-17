---
name: pmcro-maui-dependency-injection
description: Register .NET MAUI pages, page models and services with lifetimes that match how the shell actually resolves them.
---

# pmcro-maui-dependency-injection

Use for `MauiProgram`, service registration, page/page-model wiring, `HttpClient` setup, and
`AddTransientWithShellRoute`.

## PMCR-O extension

Read the whole `CreateMauiApp` before adding to it. The common defect is not a missing
registration but a *duplicated* one with a conflicting lifetime - a page model registered as a
singleton and again transiently through `AddTransientWithShellRoute`, so which instance a page
receives depends on how it was navigated to.

Rules:

- one registration per type; if a route is needed for an already-registered model, register the
  route alone (`Routing.RegisterRoute`) rather than a second lifetime
- pages resolved through `ContentTemplate` come from the container, so their constructor
  dependencies must be registered too
- tab-hosted pages are effectively singletons for the app's life; detail pages pushed onto a stack
  should be transient
- per-machine configuration (an endpoint, a local port) belongs in `Preferences`, read at
  registration time, not compiled into the binary
- never construct a service inline in page code-behind once it is in the container - two
  independently constructed clients diverge in configuration the moment one of them is changed

## Evidence to record

The diff of `MauiProgram`, plus a launch that reaches every registered page. A registration defect
usually surfaces as a resolution exception at navigation time, not at build time.
