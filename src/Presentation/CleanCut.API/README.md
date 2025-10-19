# CleanCut.API - Web API Presentation Layer

## Purpose in Clean Architecture

The **API Layer** is the HTTP/REST surface for the application. It accepts HTTP requests, maps them to Application Layer commands/queries, and returns well-formed responses. Keep controllers thin; delegate business logic to the Application Layer.

## Key Principles

- Thin controllers: routing, model binding, response formatting only.
- Clear route-based versioning (see Versioning section).
- Consistent error responses (Problem Details + global exception handling).
- Serve a small static landing page at `/versions.html` for quick endpoint discovery.

## Folder Structure (high-level)
```
CleanCut.API/
??? Controllers/         # API controllers organized by domain
?   ??? CustomersController.cs
?   ??? OrdersController.cs
?   ??? ProductsController.cs
?   ??? AuthController.cs
??? Middleware/          # Custom middleware components
?   ??? ExceptionMiddleware.cs
?   ??? RequestLoggingMiddleware.cs
?   ??? PerformanceMiddleware.cs
??? Filters/             # Action filters and attributes
?   ??? ValidationFilter.cs
?   ??? AuthorizationFilter.cs
?   ??? CacheFilter.cs
??? Extensions/          # Extension methods for setup
?   ??? ServiceCollectionExtensions.cs
?   ??? ApplicationBuilderExtensions.cs
?   ??? ControllerExtensions.cs
??? Configuration/       # API-specific configuration
?   ??? SwaggerConfig.cs
?   ??? CorsConfig.cs
?   ??? ApiVersioningConfig.cs
??? Models/              # API-specific models (if needed)
?   ??? Requests/
?   ??? Responses/
?   ??? ApiResponse.cs
??? Validators/          # API input validators
    ??? CreateCustomerRequestValidator.cs
    ??? UpdateOrderRequestValidator.cs
```

## Common Mistakes to Avoid

? **Fat Controllers** - Don't put business logic in controllers
? **Inconsistent Responses** - Use consistent response formats
? **Poor Error Handling** - Don't expose internal error details
? **Missing Validation** - Always validate input
? **Inconsistent HTTP Status Codes** - Use appropriate status codes

? **Thin Controllers** - Delegate to Application Layer
? **Consistent API Design** - Follow REST conventions
? **Proper Error Handling** - Use middleware for global error handling
? **API Documentation** - Maintain up-to-date Swagger documentation
? **Versioning Strategy** - Plan for API evolution

This layer is your application's front door - make it welcoming, secure, and well-documented!

## Versioning

This project uses explicit, route-based versioning. Controllers include the version segment directly in their routes:

- Version 1 routes: `/api/v1/...` (e.g. `src/Presentation/CleanCut.API/Controllers/ProductsController.cs`)
- Version 2 routes: `/api/v2/...` (e.g. `src/Presentation/CleanCut.API/Controllers/V2/ProductsController.cs`)
- Some controllers remain unversioned (e.g. `Users` and `Countries`), exposed as `/api/users` and `/api/countries`.

Note: earlier drafts in docs showed attribute-based ApiVersion usage. The running code uses explicit route segments. Keep route contracts stable when updating controllers.

## Exposed Endpoints (summary)

- Products (v1)
  - GET    /api/v1/products
  - GET    /api/v1/products/{id}
  - GET    /api/v1/products/user/{userId}
  - POST   /api/v1/products
  - PUT    /api/v1/products/{id}
  - DELETE /api/v1/products/{id}

- Products (v2) — enhanced responses
  - GET    /api/v2/products?page={page}&pageSize={pageSize}
  - GET    /api/v2/products/{id}            (wrapped response: data, apiVersion, timestamp)
  - GET    /api/v2/products/user/{userId}?page={page}&pageSize={pageSize}
  - POST   /api/v2/products
  - PUT    /api/v2/products/{id}
  - GET    /api/v2/products/statistics

- Users (unversioned)
  - GET    /api/users
  - GET    /api/users/{id}
  - POST   /api/users
  - PUT    /api/users/{id}
  - DELETE /api/users/{id}

- Countries (unversioned)
  - GET    /api/countries
  - GET    /api/countries/{id}
  - POST   /api/countries
  - PUT    /api/countries/{id}
  - DELETE /api/countries/{id}

If you update or add controllers, update `wwwroot/versions.html` and the README to keep consumers in sync.

## Response contracts

- v1 endpoints return application DTOs directly (plain JSON object for product/user).
- v2 endpoints adopt a wrapped response format for list/item endpoints:

# Response

## Error handling

- Global exception handling and Problem Details are configured for API routes.
- Problem Details include common extensions such as `requestId`, `timestamp`, and helpful RFC links for standard status codes.
- Controllers may still return ProblemDetails or domain-specific errors for business-rule violations (422, 400, 404).

## OpenAPI / Swagger

- The app exposes OpenAPI JSON and Swagger UI in Development.
- Swagger UI: `/swagger` (configured in `Program.cs`)
- OpenAPI JSON: `/openapi/v1.json` (mapped via OpenAPI middleware)
- Use Swagger UI to explore both v1 and v2 endpoints.

## CORS

Two named CORS policies are configured to support local Blazor clients and public Swagger testing:
- `AllowBlazorApp` — restricts to configured local origins for the Blazor UI (used in development).
- `AllowSwagger` — permissive policy used for API docs/testing.

The Blazor front-end projects in this workspace call the API at the paths above (v1/v2 and unversioned) — keep `AllowBlazorApp` origins in sync with your Blazor project's dev URL.

## Static content and landing page

- The API serves a small static landing page at `/versions.html` (in `wwwroot`) that lists endpoints and quick test links.
- Program.cs redirects root (`/`) to `/versions.html` in development.

## Program.cs highlights

- Adds OpenAPI via `AddOpenApi()` and maps OpenAPI endpoints in Development.
- Adds Problem Details customization and a global exception handler.
- Seeds sample data in Development via `DatabaseSeeder.SeedAsync(...)`.
- Serves static files (so `versions.html` is accessible).

## Testing & Clients

- Blazor clients in the repo use typed API client adapters (`IProductApiService`, `IUserApiService`) that call the routes listed above.
- Integration tests should exercise both v1 (plain DTOs) and v2 (wrapped responses) to ensure backward compatibility.

## Recommended maintenance tasks (next)
1. Keep `wwwroot/versions.html` in sync with controllers (done/required changes).
2. Reconcile any leftover ApiVersion attribute examples in docs with the actual route-based approach.
3. Align `ApiConstants` / OpenAPI endpoint constants with runtime registration if you centralize OpenAPI URLs.

If you want, I will:
- update `wwwroot/versions.html` (already prepared) — next,
- update `ApiConstants` or Program.cs to make Swagger/OpenAPI paths canonical.