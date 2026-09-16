---
name: vetgest-testing
description: Use when adding or reviewing VetGest unit, integration, authorization, contract, component, or end-to-end tests.
---

# VetGest Testing Workflow

1. State the behavior under test and the smallest test layer that can prove it.
2. Cover the happy path, validation failure, unauthorized access, and not-found behavior where applicable.
3. For linked resources, test both linked and unrelated users.
4. Keep tests deterministic and independent of production Azure services.
5. Use fakes or test containers for persistence according to repository conventions.
6. Run the focused test first, then the affected solution build and broader checks.
7. Record remaining coverage gaps and external-service risks.
