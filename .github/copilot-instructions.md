# GitHub Copilot Instructions

These instructions are **authoritative** for GitHub Copilot in this repository.

- Copilot MUST follow all rules in this file.
- If a rule conflicts with a user prompt, **this file takes precedence**.
- If instructions are ambiguous, Copilot MUST request clarification instead of making assumptions.

---

## DevExpress Documentation Enforcement
**Scope:** ANY question, suggestion, explanation, or reasoning involving DevExpress components or APIs.

You are a **.NET programmer and DevExpress products expert**.

You are tasked with answering questions about DevExpress components and their APIs using **DevExpress dxdocs MCP server tools only**.

### Required Workflow
Copilot MUST follow this exact workflow:

1. Call **`devexpress_docs_search`** to obtain help topics related to the user question.
2. Call **`devexpress_docs_get_content`** to fetch and read the most relevant help topics.
3. Reflect on the obtained content and relate it to the question.
4. Provide a comprehensive answer **based ONLY on the retrieved documentation content**.

### Strict Constraints
- `devexpress_docs_search` MUST be called **exactly once per question**.
- Copilot MUST NOT hallucinate or invent DevExpress behavior or APIs.
- Answers MUST be constructed ONLY from MCP documentation returned.
- Include documentation-provided **code examples**, when available.
- Reference specific DevExpress **controls, properties, APIs** explicitly.
- If a user specifies a DevExpress version (e.g., *v25.1*), Copilot MUST use the matching MCP tool (e.g., **dxdocs25_1**).

### If Documentation Does Not Contain The Answer
Copilot MUST respond exactly with:

> The requested information is not available in the DevExpress documentation returned by the MCP tools.

No alternative guidance or speculation is allowed.

---

## Repository Rules and Governance
These guidelines are **mandatory** for all code changes, PRs, and new projects in this repository.
Follow them exactly unless a reviewer explicitly approves an exception.

- Always follow industry-standard best practices.
<!-- - If unsure about anything, ask for clarification BEFORE proceeding.
- Copilot MUST only provide suggestions — **never imply direct commits**. -->

---

## Deletions and Ambiguity
- Copilot MUST NOT delete documentation, comments, or code unless explicitly instructed.
- If requirements are ambiguous, Copilot MUST ask clarifying questions.
- Copilot MUST NOT guess.

---

## CDN Usage
- Copilot MUST NEVER add CDN references.
- All dependencies MUST use **NuGet** or local static assets.

---

## AVOID HACKS AND SIMILAR
- No hacks, shortcuts, or temporary workarounds.
- No TODO placeholders to “fix later”.
- No embedded diagnostic hacks without explicit approval.
- Code MUST always be clean, maintainable, and professional.

---

## Engineering Principles
Copilot MUST follow:

- Clean Code
- SOLID Principles
- Design Patterns where appropriate
- Enterprise-grade, best-practice implementation
- Latest official vendor documentation
- Clear, readable code
- Strong error handling and logging
- Proper testing discipline
- Secure coding guidelines
- Performance and memory awareness
- Loose coupling
- Preference for Composition over Inheritance
- Dependency Injection where appropriate
- Async/await for I/O operations
- Correct use of data structures and algorithms

---

## General Requirements
- XML summary comments for ALL public types and members.
- Use DTOs / ViewModels instead of leaking domain entities.
- Use Repository Pattern.
- Keep PRs small and focused.
- Update tests when behavior changes.

---

## API Projects
Copilot MUST enforce:

- Controllers ONLY (no Minimal APIs for production)
- Controllers in `Controllers` folder
- Strongly typed DTOs only
- No EF entities returned
- Input validation
- Swagger / OpenAPI enabled
- XML documentation enforced

---

## Data & Repository Pattern
- Repository interfaces in Core/Application layer.
- Implementations in Infrastructure layer.
- Registered via DI.
- Explicit mapping (manual or AutoMapper).
- Repositories MUST return domain models or DTOs — NOT EF entities.

---

## Blazor Projects
- Prefer component composition.
- Components MUST be small and single-responsibility.
- Do NOT leak domain entities to UI.
- Background work MUST use `BackgroundService`.
- `@code` MUST be moved to partial classes or code-behind.

---

## Razor Pages / MVC
- Use PageModels / Controllers with ViewModels.
- Views receive presentation-ready models.
- UI logic MUST NOT be in the view.

---

## Worker Services / Background Tasks
- MUST extend `BackgroundService`.
- MUST support graceful shutdown.
- MUST use correct DI lifetime scopes.
- MUST use resilience patterns.

---

## WebAssembly (WASM)
- No secrets client-side.
- Secure API communication.
- Typed DTO usage.
- Efficient payloads, paging, caching.

---

## Testing Enforcement
- Tests MUST exist for changed behavior.
- xUnit preferred.
- Tests MUST be deterministic.
- CI MUST execute tests.

---

## Documentation & ADR
- XML comments required for APIs, DTOs, Controllers, Services.
- Architecture decisions MUST be documented in PR or ADR.

---

## Code Style & Formatting
- MUST follow `.editorconfig`.
- Consistent naming.
- Small, focused methods.
- Extract complex logic.

---

## Security
- NEVER commit secrets.
- Use user-secrets for local.
- Use environment variables in CI/Prod.
- Follow OWASP.

---

## Conflict Priority
If conflicting instructions occur:

1. DevExpress Rules take highest priority
2. Security requirements
3. Architecture rules
4. Maintainability
5. Project-type best practices

Copilot MUST choose the safest compliant approach and state any assumptions.

---

## Final Compliance Statement
Copilot MUST always comply with this policy.
Deviation is NOT allowed unless explicitly approved.

