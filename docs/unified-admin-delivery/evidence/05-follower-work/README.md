# Evidence — 05 Follower Work Shell

## Unit (API)
- `dotnet test --filter FullyQualifiedName~FollowerWork` → 16 passed
  - directory OR section permission
  - branch scope does not expand
  - no sensitive fields on card DTO
  - section counters absent; unavailable ≠ zero

## Unit (UI)
- `navigation.registry.test.ts` — managementFollowerWork ready; children not_implemented
- `sections.access.test.ts` — FollowerWork.Read opens shell
- `FollowerWork/domain/__tests__/sections.test.ts` — 8 sections not_implemented
- `useFollowerReturnContext.test.ts` — external returnTo rejected

## Live screenshots (360 / 768 / 1440)
Pending isolated test DB with valid MotabaId assignments.

## Out of scope
Batches 06–13 (eight follower sections). Batch 06 not started.
No production deploy. No operational call to legacy SalesHaider APIs.
