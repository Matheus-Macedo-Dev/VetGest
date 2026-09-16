---
name: VetGest contract conventions
description: Apply to shared API and SignalR contract files.
applyTo: "src/Shared/**/*.cs"
---

- Contracts are transport models, not EF Core entities.
- Use explicit request and response models with stable names and nullable annotations.
- Include validation and error-envelope shapes where clients need predictable behavior.
- Preserve compatibility within a vertical slice; document breaking changes.
- Keep contracts free of Infrastructure, EF Core, Blazor, and browser dependencies.
