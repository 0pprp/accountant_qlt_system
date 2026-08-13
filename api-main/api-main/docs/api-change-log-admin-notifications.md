# API Change Log — Admin In-App Notifications

## 1. Summary

- New **admin-only** in-app notification APIs for branch-scoped notifications.
- Supports unread count, paginated list, single notification details, mark one as read, and mark all as read for a branch.
- Notifications are **shared per branch** (read state is not per user).
- Only notifications for branches the current user can access are returned.

---

## 2. API Changes

### New Endpoints
- `GET /api/v1/admin/notifications/unread-count`
- `GET /api/v1/admin/notifications`
- `GET /api/v1/admin/notifications/{id}`
- `PATCH /api/v1/admin/notifications/{id}/read`
- `PATCH /api/v1/admin/notifications/read-all`

### Modified Endpoints
- None

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `GET /api/v1/admin/notifications/unread-count`
- **Type:** New
- **Request changes:**
  - Query: `branchId` (required, int)
- **Response changes:**
  - `{ unreadCount: number }`

### `GET /api/v1/admin/notifications`
- **Type:** New
- **Request changes:**
  - Query pagination: `pageIndex`, `pageSize` (defaults: `1`, `10`)
  - Query filter: `filter.branchId` (required, int)
  - Optional filter: `filter.hasRead` (bool), `filter.actionType` (enum), `filter.searchTerm` (string), `filter.startDate` (date), `filter.endDate` (date)
  - Optional sorting: `sortCriteria`
- **Response changes:**
  - Standard paginated list
  - Item fields: `id`, `title`, `description`, `actionType`, `actorUserFullName`, `createdAt`, `hasRead`
  - `actionType` values: `0` = `OrderCreated`, `1` = `TransactionCreated`

### `GET /api/v1/admin/notifications/{id}`
- **Type:** New
- **Request changes:**
  - Route: `id` (int)
- **Response changes:**
  - Same fields as list item: `id`, `title`, `description`, `actionType`, `actorUserFullName`, `createdAt`, `hasRead`

### `PATCH /api/v1/admin/notifications/{id}/read`
- **Type:** New
- **Request changes:**
  - Route: `id` (int)
  - No body
- **Response changes:**
  - Success response with no data payload

### `PATCH /api/v1/admin/notifications/read-all`
- **Type:** New
- **Request changes:**
  - Query: `branchId` (required, int)
  - No body
- **Response changes:**
  - `{ updatedCount: number }`

---

## 4. Breaking Changes

- None

---

## 5. Notes

- Admin area only; no app/mobile notification endpoints.
- `branchId` is required for list, unread count, and mark-all-as-read calls.
- Read/unread state is shared for all admins in the same branch.
- If the user has no access to the branch or notification, list/count may be empty and get/mark-by-id returns not found.
- Required permissions: `Notification.Read` for GET endpoints, `Notification.Update` for PATCH endpoints.
- Mandob and Motaba roles do not have notification permissions.
