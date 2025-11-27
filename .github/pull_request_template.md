# Pull Request Template

## Short Plan (required before implementation)
- **Goal**: _One-line summary of what this PR does._
- **Approach**: _High-level steps and important design decisions._
- **Affected projects**: _List project paths changed by this PR._
- **Migration steps**: _DB/schema/config changes, if any._
- **Test strategy**: _Which tests were added/updated and how to run them._

---

## Description
Provide a concise description of the change and why it is needed.

## Implementation notes
Include any important details for reviewers (interfaces added, background services, endpoints). Use headings for major types, e.g.:

# Class: MyService
# Interface: IMyRepository
# DTO: FooDto

## Checklist (required)
- [ ] I added the PR plan above before implementing code changes.
- [ ] I added or updated unit/integration tests and they pass locally.
- [ ] XML summary comments added for public types and members.
- [ ] Code follows SOLID/clean-code principles; no hacks or temporary workarounds.
- [ ] Relevant test projects updated to reflect code changes.
- [ ] API endpoints implemented using Controllers (no Minimal APIs for non-trivial endpoints).
- [ ] Repository interfaces and implementations follow the repository pattern and are registered in DI.

## How to run tests
Explain how to run the tests relevant to your changes (dotnet test, filters, etc.).

## Notes for reviewers
Any specific areas to review carefully (security, performance, DB migrations).
