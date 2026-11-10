# StateTransitions — SalesRequest (04A + 04E)

Supersedes the extra-view parts of [StateTransitions-04A.md](./StateTransitions-04A.md). The 04A matrix for employee/admin workflow actions is unchanged except where this file says otherwise.

Status vocabulary used here and in `Acceptance-04.md`: **Implemented / Unit-tested / Integration / Live-blocked / Not-implemented / Schema-blocked**. "Integration" means handler tests on in-memory EF — never PostgreSQL, never SignalR.

## 1. Views are not statuses

A tab is a read filter. Workflow tabs map to `SalesRequestStatus` (see 04A). Extra views are filters on other columns and **overlap** the workflow tabs.

| View | Filter (server side, before pagination) | Overlaps workflow tabs | In `tab=all` | In counts total |
|------|------------------------------------------|------------------------|--------------|-----------------|
| unassigned | `IsUnassigned()` rule (see §2) | no | yes | yes |
| incoming / prepared / inspected / pending / rejected / sold | status lists from `SalesRequestTabMap.StatusesForTab` | no | yes | yes |
| all | union of the workflow tabs the user may read | n/a | n/a | = sum of workflow tabs only |
| **sent** | `!IsDeleted AND SourceType IN (EmployeeSubmitted, Follower, Delegate)` | yes | **no** | **no** (reported under `extraViews`, `overlapping=true`) |
| **exceptions** | `!IsDeleted AND live hold AND Status NOT IN (Rejected, Completed, LegalFollowup)`; optional `exceptionStatus` filter | yes | **no** | **no** |
| **deleted** | `IsDeleted = true` | no (deleted rows are excluded from every other view) | **no** | **no** |
| legal | **Not-implemented** — see §5 | — | — | — |

Live hold = `ExceptionHoldStatus` is `Pending`, `Rejected`, or `Approved` with `ExceptionConsumedAtUtc IS NULL`.
An `Approved` hold with `ExceptionConsumedAtUtc` set is **consumed**: history only, not a hold, not shown in `exceptions`.

## 2. Exception hold lifecycle (04E)

Enum values that exist: `SalesRequestExceptionHoldStatus` = Pending(0), Approved(1), Rejected(2). No new enum values were added; "consumed" is the new column `ExceptionConsumedAtUtc`.

| From | Action | To | Actor / permission | Guards | Writes | Timeline |
|------|--------|----|--------------------|--------|--------|----------|
| no live hold (none, Rejected, or consumed Approved) | RequestException | Pending (status unchanged, assignee cleared, consumed/decision columns reset) | `RequestException` | status not in Completed/Rejected/PreparedForSale/InProgress/ConvertedToSale | hold, `TargetEmployeeId=null`, stamp++ | ExceptionRequested |
| Pending | **ApproveException** | Approved (unconsumed) | `ExceptionDecide` (separate from `RequestException`) | reason required; decider ≠ requester (requester = latest ExceptionRequested timeline actor; unknown ⇒ refuse); status not terminal; not deleted | `ExceptionDecidedByUserId`, `ExceptionDecidedAtUtc`, stamp++ | ExceptionApproved |
| Pending | **RejectException** | Rejected | `ExceptionDecide` | reason required; status not terminal | same decision columns, stamp++ | ExceptionRejected |
| Approved (unconsumed) | Assign / Reassign | stays Approved, `ExceptionConsumedAtUtc = now` | `Assign` | eligible employee | consume + assignment in the **same SaveChanges** (stamp-guarded) | Assigned (or Reassigned) + ExceptionConsumed (same OperationId) |
| Approved (consumed) | Assign / Reassign | no hold effect | `Assign` | normal rules | — | normal |
| Pending | Assign / Reassign | refused `SalesRequest_Exception_Pending_Blocks_Assign` | — | — | — | — |
| Rejected | Assign / Reassign | refused `SalesRequest_Exception_Rejected_Blocks_Assign` | — | request a new exception or reject the request | — | — |

Rules that make reuse impossible:

1. Consumption is written with the assignment, guarded by `ConcurrencyStamp`; two concurrent assigns → the loser gets `SalesRequest_Concurrency_Conflict`.
2. `ConsumeApprovedException` returns `false` when already consumed; `AssignGate` returns `Free` afterwards.
3. A new `RequestException` resets decision and consume columns, so the next approval is a new decision.
4. `IsUnassigned()` and the `UnassignedInScope` query treat a consumed hold as no hold.

Behaviour change in 04E: Pending and Rejected holds now block Assign/Reassign (previously the hold status did not gate assignment). `HoldForException` does not change `Status`; it only sets the hold, clears the assignee and resets the decision/consume columns.

## 3. Soft delete and restore (04E)

| From | Action | To | Permission | Guards | Effects |
|------|--------|----|------------|--------|---------|
| any non-deleted | **SoftDelete** | `IsDeleted=true`; **status and assignee unchanged** | `SoftDelete` | reason required (≤500); not already deleted; Status ≠ Completed; `LinkedOrderId` is null | `DeletedAtUtc`, `DeletedByUserId`, `DeleteReason`, stamp++, timeline SoftDeleted, outbox Invalidation |
| deleted | **Restore** | `IsDeleted=false` | `Restore` | must be deleted; refuse if request has `LinkedOrderId`, is non-terminal, has an assignee and that assignee is no longer eligible | see below |

Restore never grants anything new:

| Situation at restore | Result |
|----------------------|--------|
| Terminal status (Completed / Rejected / LegalFollowup) | status and assignee kept; never reopened |
| Non-terminal, assignee still eligible | status and assignee kept exactly |
| Non-terminal, assignee set but no longer eligible per the assignable-employee rules | assignee cleared, `Status = New`, `ViewedAtUtc = null` → request returns to **unassigned**; marker `assignee_cleared` in timeline reason |
| Hold `Approved` and unconsumed | downgraded to `Pending`, decision columns cleared (a stale approval is not revived); marker `approved_exception_downgraded_to_pending` |
| Hold `Approved` and consumed | stays consumed |
| Hold Pending / Rejected | unchanged |

No financial cascade: delete and restore never touch Orders, Customers, installments, safes or transactions. A request linked to an Order cannot be soft-deleted, because `CompleteSalesWorkflow` ignores deleted linked requests and the sale could no longer complete its request.

## 4. Tab membership of a held request

`SalesRequestAdminQuery.TabKeyFor` is the single source of the `tabKey` returned to the panel. It applies the live-hold rule (a held request with no assignee is not `unassigned`). A held request whose `Status` is e.g. `Assigned` still matches the `incoming` status filter **and** the `exceptions` view; the panel therefore suppresses the "moved to tab" message while an overlapping view is open.

## 5. Legal (`LegalFollowup`) — Not-implemented

`SalesRequestStatus.LegalFollowup` exists, and the domain only uses it as a terminal guard (excluded from unassigned, employee transitions and the exceptions view). **No handler or entity method transitions a request into or out of it.** The instruction was to implement `legal` only if transition rules already exist; they do not, so:

- `tab=legal` returns `SalesRequest_Tab_Not_Implemented`; no `TabLegal` permission was created.
- The panel keeps «القانونية» locked.
- To enable it later, a decision on entry/exit rules is needed first; the list tab itself would then be `Status == LegalFollowup`.

## 6. Row visibility (detail and timeline)

`SalesRequestAdminQuery.CanReadRow` — a user may open a row if any of these hold:

1. row is deleted ⇒ needs `TabDeleted`;
2. the row's workflow tab key (§4) is readable and not `legal`;
3. the row has a live hold on a non-terminal status ⇒ `TabExceptions` suffices;
4. the row's `SourceType` is in the sent set ⇒ `TabSent` suffices.

Timeline additionally needs `TimelineRead`.

## 7. Timeline event types (append-only)

| Value | Name | Added |
|-------|------|-------|
| 1–12 | Assigned … ExceptionRequested | 04A |
| 13 | ExceptionApproved | 04E |
| 14 | ExceptionRejected | 04E |
| 15 | ExceptionConsumed | 04E |
| 16 | SoftDeleted | 04E |
| 17 | Restored | 04E |

Outbox `ChangeType` is unchanged: exception decisions, delete and restore publish `Invalidation` (clients refetch). See `Realtime-04D.md`.
