# Sources-04E — SalesRequest SourceType and the `sent` view

## 1. Finding

`SalesRequestSourceType` exists as an enum (`byte`), and `SalesRequest`'s constructor accepts it, **but no production code constructs a `SalesRequest`**: there is no create/import/submit command, controller or job in `api-main/src`. Requests exist today only via manually applied SQL / tests. Consequently **no SourceType has a working ingestion path**.

The brief asked `sent` to filter by source types "that already exist in the enum AND have ingestion". The first condition holds for three values; the second holds for none. The tab was still wired (the filter, permission, counts and UI are real and tested) but it will be **empty in production until an ingestion path is built**. This is flagged in the counts response (`caveat: "no_ingestion_path_wired"`) and is a judgment call to confirm.

## 2. SourceType matrix

| Value | Name | Interpretation (enum has no per-value docs; inferred from names) | In `SentSourceTypes` (shows in tab `sent`) | Enum exists | Ingestion path wired | Unit-tested | Status |
|-------|------|-----------------------------------|-----------------------------------------------|-------------|----------------------|-------------|--------|
| 0 | Manual | created by an admin/operator by hand | No (not an inbound/"sent" source) | Yes | **No** | Filter only (excluded from sent) | Not-implemented (ingestion) |
| 1 | EmployeeSubmitted | field employee submitted the request | **Yes** | Yes | **No** | Filter + permission rules | Implemented (filter) / Not-implemented (ingestion) |
| 2 | Follower | submitted by a follower | **Yes** | Yes | **No** | Filter + permission rules | Implemented (filter) / Not-implemented (ingestion) |
| 3 | Delegate | submitted by a delegate | **Yes** | Yes | **No** | Filter + permission rules | Implemented (filter) / Not-implemented (ingestion) |
| 4 | ExcelImport | bulk import | No | Yes | **No** (no importer) | No | Not-implemented |
| 5 | ExternalCrm | external CRM sync | No | Yes | **No** (no integration) | No | Not-implemented |

`SalesRequestTabMap.IngestionWiredSourceTypes` is an explicit **empty list**; when an ingestion path is built, add its source there and extend tests. `SentSourceTypes` is the filter definition; `IngestionWiredSourceTypes` is the honesty marker.

## 3. Rules of the `sent` view

| Rule | Detail |
|------|--------|
| Filter | `IsDeleted = false AND SourceType IN (1, 2, 3)` (any status, any assignment) |
| Not a status | No new `SalesRequestStatus`; the request keeps its workflow status and also appears in its workflow tab |
| Overlap | Appears in addition to workflow tabs; excluded from `tab=all` and from the counts total |
| Permission | `SalesRequest.TabSent` (list), plus `CanReadRow` lets a TabSent-only user open sent rows |
| Branch scope | Same branch rules as other admin tabs |
| Counts | `extraViews.sent` with `overlapping=true`, `includedInAll=false`, `caveat="no_ingestion_path_wired"` |
| Employee app | `sent` is not an employee tab |
| Realtime | `TabSent` counts for hub branch-group eligibility |

Excel/CRM/Manual sources are intentionally excluded: they are not "sent by a person" and have no write path either.

## 4. What is needed to make `sent` real

1. A command/endpoint (or job) that creates `SalesRequest` rows with each source type, with branch/province validation, an initial timeline event (no `Created` event type exists yet) and an outbox change event.
2. Decide whether `Manual` belongs in the sent set.
3. Add the wired sources to `IngestionWiredSourceTypes` and drop the caveat.
4. Integration test against PostgreSQL.

Until then: **sent = Implemented (read side) / Unit-tested / Live-blocked / ingestion Not-implemented.**
