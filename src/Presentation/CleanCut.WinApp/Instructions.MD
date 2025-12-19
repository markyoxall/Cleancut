# How to Implement a New Order View in CleanCut.WinApp

This guide explains how to add a new Order List and Order Edit view to the CleanCut.WinApp project, following the same MVP and DI-driven architecture as Customers and Products. You will create all the necessary interfaces, forms, presenters, and mappings so that order data can be displayed and edited in the WinForms client.

---

## 1. Understand the Data Model

- **Reference:** `OrderInfo.cs` in `CleanCut.Application.DTOs`
- **Purpose:** This DTO defines the data you will display and edit in the UI. It includes properties like `Id`, `OrderNumber`, `OrderDate`, `Status`, `TotalAmount`, `CustomerName`, etc.

---

## 2. Create View Interfaces

### a. `IOrderListView.cs`
- **Location:** `Views/Orders/`
- **Purpose:** Defines the contract for the Order List form. It must include all events and methods needed for full MVP support, including inherited methods from `IView`.
- **Required Members:**
  - `event EventHandler? AddOrderRequested;`
  - `event EventHandler<Guid>? EditOrderRequested;`
  - `event EventHandler<Guid>? DeleteOrderRequested;`
  - `event EventHandler? RefreshRequested;`
  - `event EventHandler<Guid>? ViewOrdersByCustomerRequested;`  *(if filtering by customer is required, as in ProductListView)*
  - `void DisplayOrders(IEnumerable<OrderInfo> orders);`
  - `void ClearOrders();`
  - `Guid? GetSelectedOrderId();`
  - `void SetAvailableCustomers(IEnumerable<CustomerInfo> customers);` *(if filtering by customer is required)*
  - **Inherited from `IView`:**
    - `void Show();`
    - `void Hide();`
    - `void Close();`
    - `void ShowError(string message);`
    - `void ShowInfo(string message);`
    - `void ShowSuccess(string message);`
    - `bool ShowConfirmation(string message);`
    - `void SetLoading(bool isLoading);`

### b. `IOrderEditView.cs`
- **Location:** `Views/Orders/`
- **Purpose:** Defines the contract for the Order Edit form. It must include all events and methods needed for full MVP support, including inherited methods from `IView`.
- **Required Members:**
  - `event EventHandler? SaveRequested;`
  - `event EventHandler? CancelRequested;`
  - `OrderEditViewModel GetOrderData();`
  - `void SetOrderData(OrderEditViewModel order);`
  - `Dictionary<string, string> ValidateForm();`
  - `void ClearForm();`
  - `void SetAvailableCustomers(IEnumerable<CustomerInfo> customers);` *(if the order is associated with a customer)*
  - **Inherited from `IView`:**
    - `void Show();`
    - `void Hide();`
    - `void Close();`
    - `void ShowError(string message);`
    - `void ShowInfo(string message);`
    - `void ShowSuccess(string message);`
    - `bool ShowConfirmation(string message);`
    - `void SetLoading(bool isLoading);`

---

## 3. Create View Models

### a. `OrderEditViewModel.cs`
- **Location:** `Views/Orders/`
- **Purpose:** A simple class representing the editable fields for an order (mirroring `OrderInfo` but only for fields that can be edited).
- **Example Properties:**
  - `public string OrderNumber { get; set; }`
  - `public DateTime OrderDate { get; set; }`
  - `public Guid CustomerId { get; set; }`
  - `public string ShippingAddress { get; set; }`
  - `public string BillingAddress { get; set; }`
  - `public string? Notes { get; set; }`
  - `public List<OrderLineItemEditViewModel> OrderLineItems { get; set; }`

---

## 4. Implement the Forms

### a. `OrderListForm.cs`
- **Location:** `Views/Orders/`
- **Purpose:** Implements `IOrderListView`. Handles UI logic for displaying a list of orders, raising events for add/edit/delete/refresh/filter, and showing order details.
- **Key Points:**
  - Wire up all required button clicks to the interface events (`AddOrderRequested`, `EditOrderRequested`, `DeleteOrderRequested`, `RefreshRequested`, and `ViewOrdersByCustomerRequested` if needed).
  - Implement `DisplayOrders` to populate the list view.
  - Implement `ClearOrders` to clear the list.
  - Implement `GetSelectedOrderId` to return the selected order's ID.
  - Implement `SetAvailableCustomers` if filtering by customer is supported.
  - Implement all inherited `IView` methods for user feedback and loading state.

### b. `OrderEditForm.cs`
- **Location:** `Views/Orders/`
- **Purpose:** Implements `IOrderEditView`. Handles UI logic for editing/creating an order.
- **Key Points:**
  - Wire up Save/Cancel buttons to the interface events.
  - Implement `GetOrderData` and `SetOrderData` to transfer data between the form and the presenter.
  - Implement `ValidateForm` to check required fields.
  - Implement `ClearForm` to reset the form.
  - Implement `SetAvailableCustomers` if the order is associated with a customer.
  - Implement all inherited `IView` methods for user feedback and loading state.

---

## 5. Implement the Presenters

### a. `OrderListPresenter.cs`
- **Location:** `Presenters/`
- **Purpose:** Orchestrates the logic for the order list view. Handles loading orders, responding to add/edit/delete/refresh events, and opening the edit dialog.
- **Key Points:**
  - Subscribe to view events in `Initialize()`.
  - Use MediatR queries to load orders from the application layer.
  - Use a view factory to create the edit form and presenter.
  - After add/edit/delete, refresh the order list.

### b. `OrderEditPresenter.cs`
- **Location:** `Presenters/`
- **Purpose:** Orchestrates the logic for the order edit view. Handles saving or updating an order.
- **Key Points:**
  - Subscribe to Save/Cancel events.
  - On Save, validate the form, map the view model to a DTO, and send the appropriate MediatR command.
  - On success, close the dialog with `DialogResult.OK`.

---

## 6. Register Views and Presenters in DI

- **Location:** `Infrastructure/ServiceConfiguration.cs`
- **Purpose:** Register your new views, presenters, and factories so they can be resolved via DI.
- **Example:**
  - `services.AddScoped<IOrderListView, OrderListForm>();`
  - `services.AddScoped<IOrderEditView, OrderEditForm>();`
  - `services.AddScoped<OrderListPresenter>();`
  - `services.AddScoped<OrderEditPresenter>();`
  - Register a view factory for `IOrderEditView`.

---

## 7. Update MainForm and Navigation

- **Location:** `MainForm.cs`
- **Purpose:** Add a method to load the order management view, similar to `LoadCustomerManagement` and `LoadProductManagement`.
- **Example:**
  - Add a menu/button for "Orders".
  - Implement `LoadOrderManagement()` to resolve `IOrderListView` and `OrderListPresenter`, initialize them, and show the form as an MDI child.

---

## 8. Add AutoMapper Mappings

- **Location:** `CleanCut.Application\Mappings\MappingProfile.cs` or a new `OrderMappingProfile.cs`
- **Purpose:** Ensure mappings exist between domain entities, DTOs, and view models.
- **Example:**
  - `CreateMap<Order, OrderInfo>();`
  - `CreateMap<OrderInfo, OrderEditViewModel>();`
  - `CreateMap<OrderEditViewModel, OrderInfo>();`

---

## 9. Implement MediatR Commands and Queries (if needed)

- **Location:** `CleanCut.Application\Commands\Orders\` and `CleanCut.Application\Queries\Orders\`
- **Purpose:** Ensure there are commands for creating/updating orders and queries for listing/fetching orders.
- **Example:**
  - `CreateOrderCommand`, `UpdateOrderCommand`
  - `GetAllOrdersQuery`, `GetOrderQuery`

---

## 10. Test the New Views

- **Location:** `tests/UnitTests/CleanCut.WinApp.Tests`
- **Purpose:** Add unit tests for your presenters and DI registration, following the pattern for Customers and Products.

---

## Summary Table

| Step | File/Folder | What to Add/Change | Why |
|------|-------------|--------------------|-----|
| 1 | DTOs | Use `OrderInfo` | Defines order data |
| 2 | Views/Orders | `IOrderListView`, `IOrderEditView` (with all required events and methods, including inherited ones) | Contracts for forms |
| 3 | Views/Orders | `OrderEditViewModel` | Data for edit form |
| 4 | Views/Orders | `OrderListForm`, `OrderEditForm` (implement all interface members) | UI implementation |
| 5 | Presenters | `OrderListPresenter`, `OrderEditPresenter` (subscribe/unsubscribe to all events) | UI orchestration |
| 6 | Infrastructure | Register in DI | Enable DI resolution |
| 7 | MainForm | Add navigation | User can open order views |
| 8 | Application Mappings | Add AutoMapper profiles | Data mapping |
| 9 | Application Commands/Queries | Ensure MediatR handlers | Data access |
| 10 | Tests | Add presenter/DI tests | Ensure correctness |

---

**Tip:** Always follow the MVP pattern: keep UI logic in the view, orchestration in the presenter, and data access in the application layer. Use DI and factories for testability and maintainability.

---

You can now assign this document to a junior developer to implement a new Order view with the same functionality and structure as Customers and Products.

---
