# SOLID Principles — Concrete Examples from CleanCut.WinApp

This document provides real, concrete examples from the `CleanCut.WinApp` project that demonstrate each SOLID principle. For each principle you'll find:
- a short definition,
- one or more concrete examples from the codebase (with file paths),
- why the example satisfies the principle, and
- practical notes or caveats.

This replaces the earlier, high-level list with actionable examples you can inspect in the repo.

---

## Single Responsibility Principle (SRP)

Definition: A class should have one, and only one, reason to change. Keep responsibilities small and focused.

Example 1 — preferences file reader
- File: `src/Presentation/CleanCut.WinApp/Services/Management/UserPreferencesService.cs`
- Responsibility: load user preferences from the filesystem and deserialize JSON.
- Why SRP: this class only knows how to read preferences; it does not own presentation logic or lifecycle management. If storage changes (file → DB), only this class needs to change.

Example 2 — cache invalidation facade
- File: `src/Presentation/CleanCut.WinApp/Services/Caching/CacheManager.cs`
- Responsibility: provide high-level invalidation methods (customers/products/countries/orders).
- Why SRP: CacheManager orchestrates invalidation only — low-level cache storage is delegated to `ICacheService`.

Practical note:
- Designer-generated WinForms files are UI concerns and intentionally include UI wiring; they are allowed to be heavier than SRP for framework reasons. Keep domain/service classes focused.

---

## Open/Closed Principle (OCP)

Definition: Software entities should be open for extension but closed for modification.

Concrete example — cache invalidation and decorator
- Abstraction: `src/Presentation/CleanCut.WinApp/Services/Caching/ICacheManager.cs`
- Default impl: `src/Presentation/CleanCut.WinApp/Services/Caching/CacheManager.cs`
- Decorator added: `src/Presentation/CleanCut.WinApp/Services/Caching/LoggingCacheManager.cs`
- DI registration (decorator): `src/Presentation/CleanCut.WinApp/Infrastructure/ServiceConfiguration.cs` registers `CacheManager` and then provides `ICacheManager` as a `LoggingCacheManager` wrapping `CacheManager`.

Why this demonstrates OCP:
- To add logging around invalidation we did not change presenters or CacheManager; we added a new class (`LoggingCacheManager`) and wired it in DI. Presenters still depend on `ICacheManager` and receive the decorated instance.
- Similarly, to change invalidation strategy you can add a new implementation (e.g., `DatabaseCacheManager`) and register it in DI — no caller changes.

How to extend safely:
- Prefer small decorators that call an inner implementation and add behavior before/after delegating (logging, metrics, retry).
- Register the concrete implementation with DI and then register the decorator as the service consumers receive.

---

## Liskov Substitution Principle (LSP)

Definition: Subtypes must be substitutable for their base types without altering program correctness.

Focused example — `ICacheManager` / `CacheManager` / `LoggingCacheManager` / `Memory` vs `Redis` cache
- Interfaces and implementations:
  - `ICacheManager` (contract)
  - `CacheManager` (orchestrates invalidation via `ICacheService`)
  - `LoggingCacheManager` (decorator) — wraps an `ICacheManager` and delegates.
  - `MemoryCacheService` and `RedisCacheService` (in `CleanCut.Infrastructure.Caching`) implement `ICacheService` used by `CacheManager`.

Why LSP matters here:
- Any `ICacheManager` implementation must perform the documented effect: e.g., `InvalidateCustomersAsync` removes customer-related cache entries. If one impl required callers to call an extra `Flush()` method first, substitution would break callers — a LSP violation.
- The `LoggingCacheManager` preserves the contract by delegating to the inner `ICacheManager` and not changing preconditions or results.

Practical enforcement (recommended):
- Add a small contract test that every `ICacheManager` implementation must pass. Example outline:
  1. Seed a cache key (e.g., `CacheKeys.AllCustomers()`).
  2. Call `cacheManager.InvalidateCustomersAsync()`.
  3. Assert the key no longer exists (`ICacheService.ExistsAsync(...) == false`).
- Place contract tests in `tests/UnitTests/ContractTests` and run them against `CacheManager`, decorated instances, and any alternative implementations.

---

## Interface Segregation Principle (ISP)

Definition: Clients should not be forced to depend on interfaces they do not use. Prefer many small, focused interfaces over a fat one.

Concrete examples from WinApp:
- `IUserPreferencesService` (`src/Presentation/CleanCut.WinApp/Services/Management/IUserPreferencesService.cs`) — single method `LoadPreferencesAsync`.
- `ICacheManager` (`src/Presentation/CleanCut.WinApp/Services/Caching/ICacheManager.cs`) — exposes only invalidation methods presenters need.
- `IView` and view-specific interfaces (e.g., `ICustomerListView`, `IProductListView`) — presenters depend only on the view contract they interact with.

Why ISP helps here:
- Presenters can be unit-tested by mocking the small interfaces they actually consume.
- Implementers of these small interfaces are simpler to write and reason about.

---

## Dependency Inversion Principle (DIP)

Definition: High-level modules should not depend on low-level modules; both should depend on abstractions.

Concrete example — loader and preferences
- High-level: `ManagementLoader` (`src/Presentation/CleanCut.WinApp/Services/Management/ManagementLoader.cs`) needs preferences.
- Abstraction: `IUserPreferencesService`.
- Low-level: `UserPreferencesService` (file-based) implements the abstraction.

Why DIP is satisfied:
- `ManagementLoader` depends on `IUserPreferencesService` not `File` APIs. To change storage to DB, implement `IUserPreferencesService` and update DI registration — no change to `ManagementLoader`.

Other DIP uses in WinApp:
- Presenters depend on `ICacheManager` (abstraction) and `ICacheService` (application-level abstraction) rather than `MemoryCacheService` or Redis types.
- `CommandFactory` exposes creation helpers for commands so presenters don't construct application command objects directly.

---

## Practical checklist: where SOLID is used and where to pay attention

- Good:
  - `UserPreferencesService` (SRP/DIP)
  - `ICacheManager` + `CacheManager` (SRP/DIP/ISP)
  - `LoggingCacheManager` decorator (OCP demonstrated)
  - Presenters depend on small interfaces (ISP + testability)

- Pay attention:
  - WinForms designer files are framework-generated and inherently coupled — accept that they will not be pure SOLID. Keep logic out of designer files.
  - Ensure any future `ICacheManager` implementations adhere to the contract; add contract tests to prevent LSP regressions.

---

## Appendix: concrete code references (quick list)
- `src/Presentation/CleanCut.WinApp/Services/Management/UserPreferencesService.cs` — SRP, DIP
- `src/Presentation/CleanCut.WinApp/Services/Management/IUserPreferencesService.cs` — ISP, DIP
- `src/Presentation/CleanCut.WinApp/Services/Caching/ICacheManager.cs` — ISP, DIP
- `src/Presentation/CleanCut.WinApp/Services/Caching/CacheManager.cs` — SRP, DIP
- `src/Presentation/CleanCut.WinApp/Services/Caching/LoggingCacheManager.cs` — OCP (decorator)
- `src/Presentation/CleanCut.WinApp/Infrastructure/ServiceConfiguration.cs` — DI wiring for decorator
- `src/Presentation/CleanCut.WinApp/MVP/BasePresenter.cs` — SRP/OCP (shared presenter concerns)

---

If you want, I can take the next steps now:
1. Add the LSP contract tests for `ICacheManager` and wire them into CI/test projects.
2. Add XML documentation comments to more public classes (presenters, factories) to document contracts and expectations.
3. Implement a small example `DatabaseUserPreferencesService` to show OCP with a real alternate implementation.

Which of these shall I do next?
