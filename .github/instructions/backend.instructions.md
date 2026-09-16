---
name: VetGest backend conventions
description: Apply to ASP.NET Core, EF Core, Identity, API, SignalR, and backend test files.
applyTo: "src/Backend/**/*.cs,tests/**/*.cs,**/*.csproj"
---

- Keep Domain independent of Infrastructure and ASP.NET Core.
- Keep use cases in Application; keep HTTP concerns in API.
- Keep API controllers endpoint-only: route/action methods may bind requests, apply HTTP authorization metadata, delegate to Application services, and map results to HTTP responses. Do not place private helper methods, nested classes, domain/business validation, calculations, persistence queries, or reusable mapping logic in controllers.
- Extract authenticated-user access and resource authorization into shared API/application abstractions instead of duplicating private controller helpers.
- Use resource-level authorization for every read and write path.
- Treat EF Core global query filters as a default boundary, not the complete security policy.
- Validate invitation expiry, single use, revocation, and ownership transitions.
- Do not return persistence entities directly from controllers.
- Add focused tests for authenticated, unauthenticated, linked, and unlinked users.
