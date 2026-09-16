---
name: Review VetGest authorization
description: Review a VetGest feature for authentication, role authorization, resource isolation, SignalR access, and sensitive-data exposure.
agent: VetGest .NET Backend and Azure Architect
---

Review this scope: ${input:scope}

Prioritize concrete security findings by severity. Check anonymous, wrong-role, linked-user, unrelated-user, and administrative paths. Inspect query filters, commands, logs, caches, and SignalR groups. Recommend focused tests for each finding.
