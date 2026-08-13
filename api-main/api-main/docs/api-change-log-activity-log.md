# API Change Log — Activity Log (Admin Audit Trail)

## 1. Summary

- New **read-only admin API** to browse branch-scoped activity logs (user actions, client metadata, linked entity).
- Logs are created **automatically on the server** when write operations succeed — no new client write endpoints.
- Access requires permission **`ActivityLog.Read`**; results are limited to branches in the user’s JWT `branchIds`.

---

## 2. API Changes

### New Endpoints
- `GET /api/v1/admin/activity-logs`
- `GET /api/v1/admin/activity-logs/{id}`

### Modified Endpoints
- None

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `GET /api/v1/admin/activity-logs`
- **Type:** New
- **Request changes:**
  - Query — pagination: `pageIndex`, `pageSize` (defaults: `1`, `10`)
  - Query — **required** filter: `filter.branchId` (int)
  - Query — optional filters: `filter.activityType`, `filter.targetEntityType`, `filter.userId`, `filter.searchTerm`, `filter.startDate`, `filter.endDate` (`DateOnly`, `YYYY-MM-DD`)
  - Query — optional: `sortCriteria` (same pattern as other admin list endpoints)
- **Response changes:**
  - `200 OK` — paginated list:
    - `items[]`: `id`, `activityType`, `description`, `userName`, `userRoles`, `targetEntityType`, `targetEntityId`, `createdAt`
    - `pageIndex`, `pageSize`, `totalCount`
  - `description` and `userRoles` are **Arabic display text** (server-provided snapshots)
  - Enum fields (`activityType`, `targetEntityType`) are **numbers**

### `GET /api/v1/admin/activity-logs/{id}`
- **Type:** New
- **Request changes:**
  - Route: `id` (int)
- **Response changes:**
  - `200 OK` — single log:
    - `id`, `activityType`, `description`, `userName`, `userRoles`, `targetEntityType`, `targetEntityId`, `ipAddress`, `userAgent`, `deviceType`, `browser`, `operatingSystem`, `branchId`, `userId`, `createdAt`
  - `404` / error code `ActivityLog_Not_Found` when missing or branch not accessible

---

## 4. Breaking Changes

- None

---

## 5. Notes

- **Permission:** `ActivityLog.Read` — assigned to BranchAccountant, BranchManager, CEO; admin roles receive it via full access
- **Branch scope:** list requires `filter.branchId`; user only sees logs for branches in their token
- **`searchTerm`:** matches `description` or `userName`
- **`deviceType` values:** `0` Desktop, `1` Mobile, `2` Tablet, `3` Bot, `4` Unknown
- **`targetEntityType` values:** `0` User, `1` Branch, `2` Customer, `3` Order, `4` OrderList, `5` Product, `6` ProductCategory, `7` InstallmentPayment, `8` Purchase, `9` Expense, `10` Safe, `11` Transaction, `12` Attachment, `13` Notification
- **`activityType`:** numeric enum (`0`–`42`); use `description` for UI labels unless you maintain a local enum map
- No create/update/delete endpoints — logging is server-side only
