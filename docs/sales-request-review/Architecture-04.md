# Architecture-04 — SalesRequest admin (04A–04E)

Scope: the SalesRequest slice of the .NET 9 API (`api-main`) and the Vue panel (`panel-admin-main`). Employee app (Flutter) is out of scope except where noted.

## 1. Layers

| Layer | Project / folder | SalesRequest content |
|-------|------------------|----------------------|
| Domain | `Domain/Entities/SalesRequestAggregate` | `SalesRequest` aggregate, `SalesRequestTimelineEntry`, `SalesRequestChangeOutbox`, enums, `RestoreOutcome` |
| Domain services | `Domain/Services/SalesRequestLifecycle` | `SalesRequestTabMap` (tab keys, status lists, extra-view sets), `SalesRequestExtraViewRules` (pure decision rules for exception/delete/restore), matching rules |
| Application | `Application/SalesRequests` | Commands (Admin/App), Queries, `Common` (errors, access, tab permissions, admin query, row mapper, exception decider), `Realtime` |
| Infrastructure | `Infrastructure/Persistence` | EF configuration (`SalesRequestConfiguration`), manual SQL only (no migrations) |
| Api | `Api/Controllers/V1/AdminArea`, `AppArea`, `Api/Hubs` | thin controllers; hub `/hubs/sales-requests` |
| Panel | `src/modules/SalesRequests` | domain (`tabs.ts`, `endpoints.ts`), composables (tab access, selection), requests, realtime client, workspace view |

Domain rules are pure and unit-tested (`SalesRequestExtraViewRules`); handlers only load, call the rules, persist, write the timeline and the outbox row.

## 2. Request pipeline (all admin commands)

1. `[HasPermission]` on the action (separate permission per action).
2. Handler re-checks permission via `ICurrentUserService` (defence in depth for the new 04E commands).
3. Branch scope check (`SalesRequestAdminQuery` / `SalesRequestAccess`).
4. Domain rule check → business `Error` (`Result` pattern; no exceptions for expected cases).
5. Mutate entity (bumps `ConcurrencyStamp`), append timeline entry, append outbox row — one `SaveWithConflictGuardAsync` → `SalesRequest_Concurrency_Conflict` on stamp clash.

## 3. Read model

`SalesRequestAdminQuery` is the single place where tab → predicate lives:

- `ApplyTab(query, tab, user)` for workflow tabs and `ExtraViewQuery` for `sent / exceptions / deleted`. The same predicates feed list, counts and (via `CanReadRow`) detail/timeline.
- `AdminSalesRequestRowMapper` builds list/detail DTOs so the three views cannot drift.
- Counts return `tabs` (workflow + `all`) and a separate `extraViews` list marked `overlapping=true`, `includedInAll=false`.
- Filters are applied server side before pagination (no client-side filtering).

## 4. Permission model

Tab-read permissions and action permissions are disjoint:

| Concern | Read (tab) | Action |
|---------|------------|--------|
| Exceptions | `TabExceptions` | `RequestException` (create hold), `ExceptionDecide` (approve/reject), `Assign` (consume approved hold) |
| Deleted | `TabDeleted` | `SoftDelete`, `Restore` |
| Sent | `TabSent` | none (read-only view) |

`RequestException` cannot approve its own hold: different permission **and** requester ≠ decider for approval. Details in `Permissions-04E.csv`. All new permissions are in `OutsideAll`, so only roles granted them (Admin by seed) can use them.

## 5. Concurrency and consistency

- Optimistic concurrency with `ConcurrencyStamp`; every state-changing method bumps it; the stamp is the realtime `Revision`.
- Exception consumption is part of the assign transaction (same save) — no separate step that could be skipped or repeated.
- Timeline is append-only; one `OperationId` groups events written by the same command.
- No cascade into financial aggregates; `SoftDelete` is refused when an Order is linked or the request is Completed.

## 6. Realtime

Transactional outbox → dispatcher → SignalR groups; ids only. See `Realtime-04D.md`.

## 7. Frontend structure (04E additions)

| Concern | Where |
|---------|-------|
| Tab registry; each tab has `implementationStatus` (`ready` / `not_implemented`) + `permission` | `domain/tabs.ts` (`sent`, `exceptions`, `deleted` are ready; `legal` locked) |
| Action keys (`ExceptionDecide`, `SoftDelete`, `Restore`) | `Auth/types/model.ts` |
| Navigation `anyOf` includes the three new tab permissions | `Core/navigation/navigation.registry.ts` |
| `canReadTab` / allowed extra views | `composables/useSalesRequestTabAccess.ts` (also honours `allowedExtraViews` from counts) |
| Extra views menu, locked state for legal | `components/RequestStatusTabs.vue` |
| Endpoint constants for the 4 new POSTs | `domain/endpoints.ts` (declared; **no UI calls them yet**) |

The panel is **not claimed complete**: there are no buttons/dialogs for approve-exception, reject-exception, soft-delete or restore, and only unit tests (vitest) cover the new frontend logic.

## 8. Known gaps (honest list)

| Gap | Impact |
|-----|--------|
| No SalesRequest ingestion path | `sent` empty in production; the whole module only shows manually inserted rows |
| `legal` has no transition rules | tab stays locked |
| Manual SQL only, ModelSnapshot gap | schema must be applied by hand in order: `SCHEMA_REQUIRED.md` → `SCHEMA_REQUIRED_04A.md` → `SCHEMA_04D_Outbox.sql` → `SCHEMA_04E.sql`; entity-loading queries fail until `SCHEMA_04E.sql` is applied |
| No real PostgreSQL / SignalR test run | everything reads "Live-blocked" |
| Admin UI actions for the 04E commands | Not-implemented |
| Deleted/Exceptions list query translation is covered only by in-memory EF | provider differences (PostgreSQL) unverified |
| Flutter app consumption of realtime/new rules | not touched |
