# Contributing Guidelines and Coding Standards

These guidelines are mandatory for all code changes, PRs, and new projects in this repository. Follow them exactly unless an explicit exception is documented in the PR and approved by reviewers.

## CDN Usage
Never, I repeat, Never automatically add CDN links for JavaScript libraries or CSS frameworks in any project.
All dependencies must be managed via NuGet packages or local static files to ensure version control, security, and offline availability.

## Quick Plan (must be included in every PR)
- Goal: short summary of what the change does.
- Approach: high-level steps and design decisions.
- Affected projects: list the projects that change.
- Migration steps: DB or configuration changes, if any.
- Test strategy: what tests were added/updated and how to run them.

Always include the PR plan before implementing code changes.

## Principles
- Follow clean code and SOLID principles.
- Prefer clarity over cleverness; never introduce hacks or shortcuts.
- Use best practices for the project type (API, Blazor, Razor Pages, WebAssembly, Worker Service).
- Write code that is easy to read, test, and maintain.

## General Requirements
- Always add XML summary comments for all public types and members (classes, interfaces, DTOs, controllers, services, etc.).
- Use individual top-level headings when documenting major types in design notes (e.g., `# Class: MyService`, `# Interface: IMyRepository`, `# DTO: FooDto`).
- Use DTOs where they separate transport concerns from domain models; use ViewModels or presentation-specific models for UI layers (MVC/MVP patterns as appropriate).
- Use the repository pattern for data access; do not expose EF Core entities from controllers or UI surfaces.
- Keep PRs focused and small. If a change spans projects, include a migration plan.
- Update unit and integration tests for any behavior changes.

## API Projects
- Use controllers for API endpoints. Do NOT use Minimal APIs for production-grade endpoints.
  - Place controllers in a `Controllers` folder and derive from `ControllerBase`.
  - Use attribute routing and explicit action names for clarity.
- Always use strongly-typed request/response DTOs; never return EF entities directly from controllers.
- Validate inputs using model validation (data annotations or FluentValidation) and return clear error responses.
- Register and expose APIs with OpenAPI/Swagger and ensure DTOs have XML comments for generated documentation.

## Data & Repository Pattern
- Encapsulate direct data access behind repository interfaces.
- Define repository interfaces in application/core projects and implement them in infrastructure projects.
- Register implementations with DI using appropriate lifetimes (typically `Scoped` for repositories interacting with EF Core DbContext).
- Repositories should return domain models or DTOs; mapping must be explicit and tested (AutoMapper or manual mapping are acceptable).

## Blazor Projects
- Prefer component composition. Keep components small and single-responsibility.
- Do not leak domain entities into the UI layer; use ViewModels or DTOs tailored for the UI.
- For background work inside server-side Blazor, use hosted services / `BackgroundService` where appropriate and create appropriate scoped DI usage.

## Razor Pages / MVC
- Use Page Models or Controllers with ViewModels instead of passing domain entities directly to views.
- Keep UI logic out of pages/views; views should receive presentation-ready data only.

## Worker Services / Background Tasks
- Implement background services by extending `BackgroundService` (do not create custom thread loops unless justified and documented in the PR plan).
- Ensure graceful shutdown, proper service scope usage, and resilience (retries/backoff where needed).

## WebAssembly (WASM)
- Keep secrets on the server; do not embed sensitive values in the WASM client.
- Communicate with APIs using typed DTOs and secure authentication tokens.
- Minimize payload sizes and apply client-side caching and paging where appropriate.

## Testing
- Add or update unit/integration tests for any behavioral change. Tests should be included in the relevant test projects.
- Use xUnit (preferred) and a mocking framework (e.g., Moq) for unit tests.
- CI must run the test suites. Tests should be deterministic and non-flaky.

## Documentation & Comments
- Public APIs, controllers, DTOs, and services require XML summary comments.
- Document architectural decisions in the PR description or include an ADR when applicable.

## Code Style and Formatting
- Follow the repository's `.editorconfig`. Maintain naming conventions, formatting, and small method sizes.
- Prefer composition over long methods; extract helper methods or classes when responsibilities grow.

## Security & Secrets
- Never commit secrets or credentials. Use `dotnet user-secrets` for local development and environment variables for CI/production.
- Follow OWASP and platform-specific security best practices.

## PR Checklist (required)
- [ ] A short plan appears in the PR description before code changes were made.
- [ ] Unit/integration tests added or updated and pass locally.
- [ ] XML summary comments added for public types and members.
- [ ] Code follows SOLID and clean code principles; no hacks or temporary workarounds.
- [ ] Relevant test projects updated to reflect code changes.
- [ ] API controllers used for endpoints (no Minimal APIs for non-trivial endpoints).

## Enforcement
- Reviewers should request changes for PRs that do not follow these rules.
- CI will enforce tests and static analysis. Maintainers will enforce other rules during review.

---

If you want, I can also:
- Create a `.github/pull_request_template.md` with the required PR plan and checklist.
- Create a `.github/copilot.yml` instruction file that summarizes these rules for GitHub Copilot usage in this repository.
- Generate PR/issue templates or an `.editorconfig` aligned with these standards.
