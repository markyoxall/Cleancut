# CleanCut.BlazorSPA (Customers Demo)

This is a small Blazor WebAssembly demo app focused on customers CRUD and input controls used for learning.

Features
- Full in-memory CRUD (Create, Read, Update, Delete) against a shared state service.
- Local persistence to browser localStorage so data survives refresh.
- A wide range of Blazor inputs demonstrated on the Customer edit page:
  - `InputText`, `InputNumber`, `InputDate`, `InputSelect`, `InputCheckbox`, `InputTextArea`, `InputFile` and more.
- FluentValidation (via `FluentValidation` + `Blazored.FluentValidation`) for client-side validation with per-field messages and debounced validation-on-change.
- Simple responsive layout and navigation.
- Templated components and `RenderFragment<T>`: `CustomerListWrapper` demonstrates how to build reusable list components.
- Component parameters and `EventCallback<T>`: `CustomerRow` shows parent/child communication.
- Client-side JS interop wrapper: `IJsInteropService` + `JsInteropService` (thin wrapper for `confirm`/`alert`).
- Confirm modal component: `Pages/Components/ConfirmModal.razor` — accessible modal used for delete confirmation without JS.
- Error boundary wrapper: `Pages/Components/ErrorBoundaryWrapper.razor` — demonstrates catching and handling rendering exceptions.
- Query parameter example: `Pages/QueryDemo.razor` shows reading and updating `?q=` query string.

How to run

1. Ensure .NET 10 SDK is installed.
2. From the project directory (this project is `CleanCut.BlazorSPA`), run:
   ```
   dotnet restore
   dotnet build
   dotnet run
   ```
3. Open the browser and navigate to the app URL shown in the console (usually `https://localhost:5001` or similar) and go to `/customerslist`.

Why these features are included
- `RenderFragment<T>` and templated components teach composition: build a single robust list component and pass row templates for different types.
- `EventCallback<T>` shows patterns for child components to notify parents about user actions without direct coupling.
- `IJsInteropService` demonstrates `IJSRuntime` usage and gives a central place to replace `confirm`/`alert` with a more advanced toast library later.
- The `ConfirmModal` demonstrates building accessible modals in Blazor without requiring JS.
- `ErrorBoundary` is used to catch exceptions in child components and render a friendly UI instead of breaking the whole app.
- Query parameters are useful for linkable searches and stateful navigation.

Notes
- This app uses an in-memory state service backed by `localStorage` for persistence. There is no server-side persistence.
- Validation rules live in `Pages/Validation/CustomerEditValidator.cs` and are enforced on the client.
- The UI is intentionally simple for demonstration and learning.

Next steps you might try
- Replace the in-memory state with a real API service.
- Add unit tests using xUnit and bUnit for component testing.
- Integrate a 3rd-party grid (Radzen, Telerik) for advanced grid features.

License: MIT (change as needed)
