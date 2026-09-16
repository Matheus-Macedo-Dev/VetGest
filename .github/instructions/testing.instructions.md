---
name: VetGest testing conventions
description: Apply to unit, integration, component, and end-to-end test files.
applyTo: "tests/**/*.cs,tests/**/*.razor,**/*.test.*"
---

- Prefer deterministic tests focused on one behavior.
- Every protected resource path needs linked and unlinked access cases.
- Test due-date calculations with mating dates, ovulation dates, missing dates, and uncertainty metadata.
- Test alert severity and emergency copy as curated business behavior.
- Do not use real credentials, private records, external production services, or live push providers in tests.
