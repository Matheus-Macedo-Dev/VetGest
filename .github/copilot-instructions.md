# VetGest AI Development Rules

## Order of work

- AI governance files must be created and reviewed before application code.
- Before editing, identify the owning abstraction, state one falsifiable hypothesis, and name one focused validation check.
- Work in small vertical slices. Validate each slice with the narrowest available test, build, or analyzer.
- Do not invent requirements. Record unresolved assumptions in the relevant documentation.

## Architecture

- Maintain two solution files: `VetGest.Backend.sln` and `VetGest.Web.sln`.
- Backend projects: Domain, Application, Infrastructure, API, and Contracts.
- Web projects: Web and the shared Contracts project.
- Domain has no infrastructure or presentation dependencies.
- The Blazor client must not reference API, Infrastructure, or Domain assemblies.
- Never expose EF Core entities directly as API contracts.
- API controllers are endpoint-only adapters: they may bind requests, declare authorization metadata, delegate to Application services, and map results to HTTP responses. They must not contain private helper methods, nested classes, business validation, calculations, persistence queries, or reusable mapping logic.

## Security

- Enforce authorization on the server at both role and resource level.
- Roles are `Vet` and `Tutor`; roles alone do not grant access to unrelated records.
- Global query filters are defense in depth, not a replacement for authorization checks.
- SignalR is not an authorization boundary; authorize connections, groups, and operations.
- Never commit secrets, real medical data, tokens, or production connection strings.

## Veterinary safety

- VetGest provides organization and education, not diagnosis, prescriptions, or treatment.
- Treat due dates as estimates and explain the calculation basis and uncertainty.
- Separate dog and cat content wherever evidence or guidance differs.
- Alerts and educational content require veterinary review.
- Emergency messages must direct users to immediate veterinary care.
- Do not create invasive home-obstetric instructions or automatic medication protocols.

## Quality

- Use the selected UI library consistently: MudBlazor.
- Prefer clear, testable code over speculative abstractions.
- Add tests for authorization, linked-record isolation, date calculations, and alert rules.
- Keep public API contracts versionable and backward-compatible within an MVP slice.
- Do not modify unrelated user changes.
