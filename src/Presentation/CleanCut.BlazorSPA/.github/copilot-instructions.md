# Copilot Instructions for CleanCut.BlazorSPA

## Project Overview

-   **Blazor WebAssembly SPA** for customer/product CRUD, demoing Blazor input controls, state, and validation patterns.
-   **In-memory state** (see `Pages/State/`) with browser `localStorage` persistence; no backend API.
-   **Key features:**
    -   CRUD for customers/products via shared state services
    -   Client-side validation using FluentValidation (`Pages/Validation/`)
    -   Templated components (`RenderFragment<T>`) for reusable lists (see `CustomerListWrapper`)
    -   Parent/child communication via `EventCallback<T>` (see `CustomerRow`)
    -   JS interop via `IJsInteropService`/`JsInteropService` (for `confirm`/`alert`)
    -   Accessible modal dialog (`Pages/Components/ConfirmModal.razor`)
    -   Error boundary wrapper for safe rendering (`Pages/Components/ErrorBoundaryWrapper.razor`)
    -   Query string handling (`Pages/QueryDemo.razor`)

## Architecture & Patterns

-   **State services**: `InMemoryCustomerState`/`InMemoryProductState` manage entity lists and sync to `localStorage`.
-   **Validation**: Validators in `Pages/Validation/` are used with `Blazored.FluentValidation` for per-field, debounced validation.
-   **Component structure**: Pages in `Pages/`, shared layouts in `Layout/`, and reusable components in `Pages/Components/`.
-   **Communication**: Use `EventCallback<T>` for child-to-parent events; avoid direct service injection in child components.
-   **JS Interop**: Use the `IJsInteropService` abstraction for all JS calls (do not use `IJSRuntime` directly in components).

## Developer Workflows

-   **Build/Run**: Use standard .NET commands:
    ```
    dotnet restore
    dotnet build
    dotnet run
    ```
-   **Testing**: (Planned) Add tests in `tests/CleanCut.BlazorSPA.Tests/` using xUnit/bUnit.
-   **Debugging**: Use browser dev tools and Blazor debugging support. State is persisted in `localStorage`.

## Conventions & Tips

-   **Do not add server-side code**; this is a client-only demo.
-   **Persist all state changes** via the state services, not directly to `localStorage`.
-   **Validation**: Always use the provided validators for forms.
-   **Component parameters**: Prefer `EventCallback<T>` for events, and use `[Parameter]` for data.
-   **Error handling**: Wrap risky UI in `ErrorBoundaryWrapper`.
-   **Modal dialogs**: Use `ConfirmModal` for confirmations, not JS `confirm` directly.

## Key Files & Directories

-   `Pages/State/`: In-memory state services
-   `Pages/Validation/`: Validators
-   `Pages/Components/`: Shared UI components
-   `Layout/`: Layout and navigation
-   `README.md`: Project overview and rationale

---

For new features, follow the patterns in existing state services, validators, and component communication. See `README.md` for rationale behind major design choices.
