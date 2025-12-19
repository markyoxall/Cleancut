# CleanCut.WinApp — Desktop Client (WinForms)

This document describes the Windows Forms (WinForms) desktop client for the CleanCut solution. It focuses on the desktop app's architecture, DI wiring, presenter/view patterns, and the unit tests that exercise the UI plumbing.

## Purpose
`CleanCut.WinApp` is a sample desktop client demonstrating:
- A lightweight desktop UI that can reuse application DTOs and business logic from the solution
- WinForms MVP-style presenters and testable view interfaces
- Local DI-driven composition so presenters and views are created in a testable manner
- Integration points with the same core application layer used by the web APIs

> Note: The WinForms client in this repo accesses the database directly (sample) and is intended as an example public client. Secure API clients in the solution use OAuth2/OIDC; the desktop client shows a different hosting model.

## High-level architecture

- UI layer uses MVP pattern:
  - `I*View` interfaces live in `src/Presentation/CleanCut.WinApp/Views/*`
  - Concrete `Form` implementations (e.g. `CustomerListForm`, `ProductEditForm`) implement these interfaces
  - Presenters live in `src/Presentation/CleanCut.WinApp/Presenters/*` and contain UI orchestration logic (loading data via MediatR, responding to user actions, opening edit dialogs)

- Dependency injection and composition
  - `ServiceConfiguration.ConfigureServices()` (in `Infrastructure`) registers views, presenters, factories and the `MainForm`.
  - Presenters are resolved and constructed through DI; where a single presenter needs a new child view, a small `IViewFactory<TView>` abstraction is used to create views on demand.
  - `ActivatorUtilities` is used to create presenter instances when passing a concrete view instance at runtime so constructor DI remains available.

- Navigation and lifecycle
  - `Services.Navigation.NavigationService` centralizes MDI navigation (open/close child forms) and presenter lifecycle (initialize, cleanup, dispose).
  - `MainForm` uses DI to obtain navigation and delegates showing the customer/product management views to the navigation service.

- Background operations and threading
  - Presenters use a helper method to execute async work and marshal UI updates back to the UI thread. Long-running refreshes are scheduled on background threads so the UI remains responsive.

## Key folders
- `Presenters/` — presenter classes that orchestrate UI logic
- `Views/` — view interfaces and WinForms `Form` implementations
- `Infrastructure/ServiceConfiguration.cs` — DI registrations for views, presenters, factories and services
- `Services/Navigation/` — navigation service that manages MDI children and presenter lifecycle
- `MVP/` — base presenter and base form abstractions

## Testing (what's included)
Unit tests for the WinForms client are located under `tests/UnitTests/CleanCut.WinApp.Tests`. They intentionally keep UI surface area small so they can run headless and fast.

Written tests (examples in this workspace):
- `NavigationServiceTests`
  - Exercises `NavigationService` to ensure navigation calls run without throwing when views are mocked.
  - Verifies that the navigation service safely attempts to close MDI children and calls presenter cleanup/dispose logic.

- `CustomerListPresenterTests`
  - Uses a compact `TestCustomerListForm` (a `Form` that implements `ICustomerListView`) plus a mocked `IMediator` to simulate data returned from the application layer.
  - Verifies that `CustomerListPresenter.Initialize()` loads customers via MediatR and calls `DisplayCustomers` on the view.

- `ProductListPresenterTests`
  - Uses mocked mediator responses and a small `IViewFactory<IProductEditView>` test factory to exercise the add/edit flows and verify `DisplayProducts` is called.

- `ServiceRegistrationTests`
  - Simple DI smoke test that calls `ServiceConfiguration.ConfigureServices()` and checks that key services (for example `MainForm` and `INavigationService`) can be resolved.

Why these tests exist
- They validate presenter/view wiring without relying on a full running application or database.
- They test DI registration and navigation semantics so regressions in application composition are caught early.

How to run the WinApp tests

From the repository root:
```bash
# run only WinApp tests
dotnet test tests/UnitTests/CleanCut.WinApp.Tests
```

Or run the full test suite:
```bash
dotnet test
```

## Contributing / Extending the WinApp
- Prefer creating small view interfaces (`I*View`) that are simple to implement and test.
- Use `IViewFactory<TView>` to create transient dialog views instead of resolving views directly from `IServiceProvider` when the view instance is created at runtime.
- Keep presenters free of direct UI code (no static MessageBox calls); move user-facing messaging to the view interface so it can be asserted in tests.
- Add unit tests for asynchronous flows by using light-weight `Form` fakes that implement the view interface and perform minimal UI work.

## Troubleshooting
- If DI resolution fails at startup, run `dotnet build` to see compile-time errors. The WinApp DI configuration is in `ServiceConfiguration.ConfigureServices()`.
- If a form does not display in MDI mode, ensure its `MdiParent` is set to the `MainForm` before calling `Show()`.

## References
- See `docs/README.md` for full solution architecture, diagrams and background services documentation.

