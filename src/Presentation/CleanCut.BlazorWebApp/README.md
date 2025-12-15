# CleanCut.BlazorWebApp — Server‑Side Blazor with OAuth2 / OIDC

`CleanCut.BlazorWebApp` is a Blazor Server application that acts as an OpenID Connect / OAuth2 (Authorization Code + PKCE) client. It provides server-side rendering with real-time interactivity and authenticated API access to `CleanCut.API`.

This README documents the current behavior, how authentication is configured, and how to run the app locally. It has been aligned to match the code in `Program.cs`, `ServiceCollectionExtensions`, and the typed HTTP clients.

## Key points
- Uses Cookie + OpenID Connect (Authorization Code + PKCE) for user authentication
- Access tokens are stored server-side (`SaveTokens = true`) and injected into API calls via `AuthenticatedHttpMessageHandler`
- Typed `HttpClient` instances are registered for API v1/v2 and other services
- UI state is managed by scoped state services (e.g., `ProductsState`, `CustomersState`)

## Runtime requirements
- IdentityServer (development): `https://localhost:5001`
- CleanCut.API: `https://localhost:7142`
- Blazor App: `https://localhost:7297`

## How to run (local development)
1. Start IdentityServer:
```bash
dotnet run --project src/Infrastructure/CleanCut.Infranstructure.Identity
```

2. Start API:
```bash
dotnet run --project src/Presentation/CleanCut.API
```

3. Start Blazor app:
```bash
dotnet run --project src/Presentation/CleanCut.BlazorWebApp
```

Open the app in the browser: `https://localhost:7297`

Note: make sure `builder.Services.AddHttpContextAccessor()` is registered in `Program.cs` so the authenticated message handler can read tokens from the current HTTP context.

## Authentication summary
- Client type: Public client (no client secret)
- Flow: Authorization Code (response_type = `code`) with PKCE
- Scopes: `openid`, `profile`, `CleanCutAPI`

## Service registration notes
- See `src/Presentation/CleanCut.BlazorWebApp/Extensions/ServiceCollectionExtensions.cs` for exact typed HTTP client and state registrations. Notable registrations:
  - `IProductApiClientV1`, `IProductApiClientV2` — typed product clients
  - `ICustomerApiService`, `ICountryApiService` — typed API adapters
  - `AuthenticatedHttpMessageHandler` — injects `Authorization: Bearer {access_token}`
  - `ICustomersState` implemented by `CustomersState`

## Troubleshooting
- If you encounter authentication loops, confirm IdentityServer is running and redirect URIs match the client configuration.
- If tokens are not attached to API calls, confirm `AddHttpContextAccessor()` and `SaveTokens = true` are configured.

## Action items
1. Fix DI typos (completed: `CustomeraState` → `CustomersState`).
2. Add unit tests for `ProductsState`, `AuthenticatedHttpMessageHandler`, and key HTTP clients.

---

This README contains the current, validated instructions for running and developing `CleanCut.BlazorWebApp`.
