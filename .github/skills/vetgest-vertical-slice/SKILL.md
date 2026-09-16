---
name: vetgest-vertical-slice
description: Use when implementing a VetGest feature from API contract through application logic, persistence, UI, and focused tests.
---

# VetGest Vertical Slice

1. Confirm the requirement and acceptance behavior.
2. Identify or define shared request and response contracts.
3. Implement the domain rule and application use case.
4. Add persistence only behind the Infrastructure boundary.
5. Add the API endpoint and authorization checks.
6. Add the Blazor client, states, and responsive UI states.
7. Add focused tests, including authorization and validation cases.
8. Run the narrowest test or build after each layer, then run both solution builds.
9. Update documentation when the contract or operational behavior changes.
