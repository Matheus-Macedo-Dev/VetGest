---
name: vetgest-security
description: Use when designing or reviewing VetGest authentication, authorization, invitations, SignalR access, EF Core filters, secrets, or private data handling.
---

# VetGest Security Review

1. Identify the user, role, resource, and operation.
2. Check authentication, role policy, resource ownership, and linked-record authorization separately.
3. Verify that query filters are defense in depth and are not the only access control.
4. Check invitation expiry, single use, revocation, token storage, and audit events.
5. Check SignalR connection, group membership, and method authorization.
6. Check logs, errors, caches, URLs, and telemetry for secrets or private records.
7. Add or update tests for linked, unlinked, anonymous, and boundary cases.
8. Report unresolved security assumptions before implementation.
