## CHANGELOG — 2025-11-27 (Diagnostics update)

- Investigated a runtime Blazor error reported on this date: `Found malformed component comment` during server component rendering.
- Actions performed / documented:
  - Performed client-side diagnostics guidance (DevTools WebSocket/SSE inspection) and reproduced error across multiple browsers to rule out extensions.
  - Reviewed server-side pipeline and common mutation points (middleware that writes to response, response post-processing, duplicate script references, and layout files).
  - Collected candidate files and locations to inspect (Blazor host/layouts, Program.cs, global middleware, and custom circuit handlers).
  - No automated code changes were applied to application runtime files as part of the investigation.
- Recommended next steps (manual):
  1. Capture the raw `/_blazor` WebSocket/SSE messages from browser DevTools and inspect the first server payload for injected or corrupted comment tokens.
  2. Temporarily disable Browser Link / CSS hot-reload and any response-rewrite middleware while testing.
  3. Grep the server projects for `Response.Write`, `WriteAsync`, `WriteAsJsonAsync`, and `Response.Body` patterns and inspect handlers that run globally.
  4. Confirm there is exactly one `<script src="_framework/blazor.web.js">` included in the host layout.
  5. If reproducing locally, run Kestrel directly (bypass any proxy) to rule out reverse-proxy modifications.

Note: The documentation was updated to capture the investigation steps and recommendations. No changes to application logic or runtime files were made by this documentation update.
