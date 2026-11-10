# Acceptance-04 — honest status matrix (04A–04E)

> **04F/04G updates:** operational close-out → `Acceptance-04F.*` + `04f-live-evidence/`. Vue Hub wiring → `Acceptance-04G.*` + `04g-live-evidence/`. Do not treat this 04A–04E snapshot as the final live matrix.

**The panel is NOT claimed complete.** This file states what was built and what evidence exists for 04A–04E before the isolated live pass.

## Status vocabulary

| Status | Meaning |
|--------|---------|
| Implemented | Code exists and builds |
| Unit-tested | Pure rules and/or handler tests pass (xUnit; vitest for the panel) |
| Integration | Handler flow verified on EF **InMemory** only — never PostgreSQL |
| Live-blocked | Needs a running environment (PostgreSQL + API + SignalR + browser) that was not exercised |
| Schema-blocked | Entity-loading queries fail until manual SQL is applied (`SCHEMA_REQUIRED.md`, `SCHEMA_REQUIRED_04A.md`, `SCHEMA_04D_Outbox.sql`, `SCHEMA_04E.sql`) |
| Not-implemented | No code path exists |

Evidence at the time of writing: `dotnet test` → 126 passed (26 in the new `SalesRequestExtraViews04ETests`); panel `vitest` and `vue-tsc --noEmit` clean. Every "Live-blocked / Schema-blocked" cell below is still unverified.

## 1. Tabs

| Tab | Kind | API | Implemented | Unit-tested | Integration | Live-blocked | Schema-blocked | Panel (read + menu) | Notes |
|-----|------|-----|-------------|-------------|-------------|--------------|----------------|---------------------|-------|
| unassigned | workflow | list/counts | Yes (04A) | Yes | — | Yes | Yes | Implemented, vitest | Consumed Approved hold no longer hides a row |
| incoming / prepared / inspected / pending / rejected / sold | workflow | list/counts | Yes (04B) | Yes (TabMap, permissions) | — | Yes | Yes | Implemented, vitest | |
| all | workflow union | list/counts | Yes | Yes | — | Yes | Yes | Implemented | Excludes extra views |
| **sent** | extra | list/counts/detail | Yes | Yes | Partly | Yes | Yes | Unlocked, vitest | Read side only; **no ingestion** → empty in production |
| **exceptions** | extra | list (+`exceptionStatus`)/counts/detail | Yes | Yes | Yes (approve/reject/consume flows) | Yes | Yes | Unlocked, vitest | Live holds on non-terminal rows |
| **deleted** | extra | list/counts/detail | Yes | Yes | Yes (delete/restore) | Yes | Yes | Unlocked, vitest | Read only |
| **legal** | extra | `SalesRequest_Tab_Not_Implemented` | **Not-implemented** | Yes (TabMap says not implemented) | — | — | — | Locked in menu | No domain rules into `LegalFollowup`; no `TabLegal` permission |

## 2. Actions

| Action | API | Unit-tested | Integration (in-memory) | Live | Schema | Panel UI |
|--------|-----|-------------|-------------------------|------|--------|----------|
| Assign (with exception gate + consume) | Implemented | Yes | Yes | Live-blocked | Schema-blocked | Existing 04A UI; new error codes not specially handled |
| Reject request | Implemented (04A) | Yes (entity) | No | Live-blocked | Schema-blocked | Existing |
| RequestException | Implemented (changed in 04E) | Yes | Yes | Live-blocked | Schema-blocked | Existing |
| **ApproveException** | Implemented | Yes | Yes | Live-blocked | Schema-blocked | **Not-implemented** (action key + endpoint constant only) |
| **RejectException** | Implemented | Yes | Yes | Live-blocked | Schema-blocked | **Not-implemented** |
| **SoftDelete** | Implemented | Yes | Yes | Live-blocked | Schema-blocked | **Not-implemented** |
| **Restore** | Implemented | Yes | Yes | Live-blocked | Schema-blocked | **Not-implemented** |

## 3. Rules asked for in the brief

| Rule | Where enforced | Evidence |
|------|----------------|----------|
| Approved hold cannot be used for assign twice | `ConsumeApprovedException` + `AssignGate`; consume in same save as assign | Unit: consume once, second call false; Integration: approve → assign stamps `ExceptionConsumedAtUtc` once and writes ExceptionConsumed |
| Pending / Rejected hold blocks assign | `EnsureExceptionAllowsAssignment`, handler gate | Unit + Integration |
| RequestException cannot approve itself | separate `ExceptionDecide` permission; requester ≠ decider; unknown requester refused | Unit |
| New RequestException resets consumption | `HoldForException` | Unit |
| Restore never re-grants illegal status | `Restore`: terminal never reopened, ineligible assignee → `New`/unassigned, stale approval → Pending, consumed stays consumed | Unit + Integration |
| No financial cascade | SoftDelete/Restore touch only `SalesRequests` + timeline + outbox; blocked when linked Order or Completed | Unit + Integration |
| Counts mark extra views | `extraViews[]` with `overlapping=true`, `includedInAll=false`, `sent.caveat` | Code review; no counts-handler test |
| Permissions in OutsideAll | `Permissions.cs` | Unit (separate permissions, row visibility) |

## 4. SourceType matrix (see `Sources-04E.md`)

| SourceType | In `sent` filter | Ingestion | Status |
|------------|------------------|-----------|--------|
| Manual (0) | No | Not wired | Not-implemented |
| EmployeeSubmitted (1) | Yes | Not wired | Implemented (filter) / ingestion Not-implemented |
| Follower (2) | Yes | Not wired | Implemented (filter) / ingestion Not-implemented |
| Delegate (3) | Yes | Not wired | Implemented (filter) / ingestion Not-implemented |
| ExcelImport (4) | No | Not wired | Not-implemented |
| ExternalCrm (5) | No | Not wired | Not-implemented |

## 5. Realtime and other cross-cutting

| Item | Status |
|------|--------|
| Outbox write for approve/reject/delete/restore (Invalidation) | Implemented, Unit/Integration (in-memory) |
| Hub branch-group eligibility includes extra-view permissions | Implemented, Unit-tested |
| Real SignalR delivery | Live-blocked |
| Panel Vue SignalR client (`salesRequestRealtime.ts`) | Implemented — Live-blocked (not measured ≤2s on a real hub) |
| Flutter SignalR client (`sales_request_realtime_client.dart`) | Implemented — Live-blocked; home bins not yet auto-wired on every screen |
| Panel auto-refresh after the new commands | Partial: hub invalidation wired on workspace; approve/delete UI buttons **Not-implemented** |
| Counts handler test | Not-implemented |
| List query translation on PostgreSQL | Live-blocked |
| Flutter 04C SalesRequest cycle (bins/detail/actions/ensure-draft) | Implemented + unit-tested; Live-blocked for device |
| Migration file | Intentionally not generated (manual SQL policy) |

## 6. Go/no-go before calling 04E done

1. Apply the four SQL files in order on an isolated PostgreSQL.
2. Run the API; exercise each endpoint in `EndpointMatrix-04E.csv`.
3. Build panel dialogs for approve/reject exception, soft delete and restore, then browser-test them.
4. Decide `sent` ingestion and `legal` rules with the business.
5. Two-client SignalR run (admin + employee).
