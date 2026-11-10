# Realtime-04D — SalesRequest realtime (with 04E additions)

Status: **Implemented + Unit-tested + Integration (in-memory EF with a fake publisher) — Live-blocked.** No SignalR connection, PostgreSQL outbox row or multi-instance run has been exercised. Table must exist first: `SCHEMA_04D_Outbox.sql`.

## 1. Flow

| Step | Component | Behaviour |
|------|-----------|-----------|
| 1 | Command handler | Writes the business change **and** one `SalesRequestChangeOutbox` row in the same `SaveChanges` (transactional outbox). |
| 2 | `SalesRequestOutboxProcessor.ProcessBatchAsync` | Batch of 50 oldest unpublished rows (`PublishedAtUtc IS NULL`, `RetryCount < 20`, last attempt older than 5 s). Publishes each, marks published; on error `MarkFailed` (RetryCount++, error type only). |
| 3 | Hosted dispatcher | Polls roughly every 1.5 s (at-least-once; consumers must be idempotent). |
| 4 | `SalesRequestHubGroups.ForChange` | Target groups = `sr:branch-admin:{branchId}` + `sr:user:{target}` + `sr:user:{previousTarget}` (when set and different). |
| 5 | Hub `/hubs/sales-requests`, method `SalesRequestChanged` | Payload only. |
| 6 | Panel `salesRequestRealtime.ts` | Dedup by EventId, drops revisions older than the latest seen per request, batches invalidation, refetches. |

## 2. Payload (ids and numbers only — no names, phones, money, notes)

`EventId, RequestId, Revision, OccurredAtUtc, ChangeType, BranchId, TargetEmployeeId, PreviousTargetEmployeeId, Audience` (`branch` or `branch+employee`).

Revision = the entity `ConcurrencyStamp` after the change.

## 3. ChangeType values in use

| ChangeType | Published by |
|------------|--------------|
| Assigned / Reassigned | assign, reassign |
| StatusChanged / NoteChanged / DraftLinked / Completed | employee app transitions, sale completion |
| Invalidation | request-exception (04A), **approve-exception, reject-exception, soft-delete, restore (04E)** |

Exception decisions, delete and restore deliberately use `Invalidation` (client refetches list, counts, detail and timeline) — no new enum or column was added. The `Assign` that consumes an approved hold publishes the normal `Assigned`/`Reassigned` event; consumption is not a separate event.

## 4. Group membership (hub connect)

| Group | Condition |
|-------|-----------|
| `sr:user:{userId}` | user has `SalesRequest.AppRead` |
| `sr:branch-admin:{branchId}` for each branch of the user | user has **any** workflow tab permission **or** any implemented extra-view tab permission (`TabSent`, `TabExceptions`, `TabDeleted`) — 04E widened this so an exceptions-only or deleted-only admin still receives invalidations |

Branch membership comes from `SalesRequestHubIdentityResolver` (not from client parameters). The client cannot subscribe to arbitrary groups.

Because the payload contains ids only, a user who receives an event for a row they cannot read will refetch and the read endpoints enforce `CanReadRow` / tab permissions; nothing sensitive is on the wire.

## 5. Client behaviour (panel)

- Reconnect schedule `[0, 1, 2, 5, 10] s`; state `connected | reconnecting | disconnected`; on reconnect it invalidates list and counts to catch missed events (there is no server-side replay).
- Open detail panel is refetched when its request id appears in an event.
- 04E adds no new client event handling; the workspace refetches whichever tab (workflow or extra) is current.

## 6. Failure semantics

| Case | Result |
|------|--------|
| Publisher throws | `RetryCount++`, retried after 5 s, up to 20 attempts, then parked for manual inspection |
| App crash between save and publish | Row stays unpublished → dispatched after restart |
| Duplicate delivery | Client dedups by EventId; invalidation is idempotent |
| Out-of-order delivery | Client ignores a revision lower than the highest seen for that request |
| Hub down / user offline | Missed events; reconnect invalidation or manual refresh recovers |

## 7. Not verified / known limits

| Item | Status |
|------|--------|
| Real SignalR end-to-end (admin + app clients) | Live-blocked |
| Outbox table on PostgreSQL | Schema-blocked (apply `SCHEMA_04D_Outbox.sql`) |
| Multi-instance dispatch (two API pods) | Not tested; both would poll the same rows (at-least-once duplicates are tolerated, no row locking implemented) |
| Retention of published rows | Manual SQL only (see schema file) |
| Employee app (Flutter) realtime consumption | Not-implemented in the panel scope; user group is published for it |
| Reconciliation/replay endpoint | Not-implemented |
