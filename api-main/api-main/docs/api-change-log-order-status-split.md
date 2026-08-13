# API Change Log — Order Status Split

## 1. Summary

- Order `status` is split into two fields: **`executionStatus`** (installment lifecycle) and **`approvalStatus`** (admin approval).
- Former `PendingApproval` maps to **`executionStatus: NotStarted`** + **`approvalStatus: Pending`**.
- Admin order list/export column key **`Status`** is renamed to **`ExecutionStatus`**.

---

## 2. API Changes

### New Endpoints
- None

### Modified Endpoints
- `GET /api/v1/app/orders/{id}`
- `GET /api/v1/admin/orders`
- `GET /api/v1/admin/orders/available-columns`
- `GET /api/v1/admin/orders/excel-report`
- `GET /api/v1/admin/orders/{id}/installment-payments`
- `PUT /api/v1/admin/orders/{id}/seller-info`
- `GET /api/v1/admin/customers/{id}/orders`
- `GET /api/v1/app/customers/{id}/orders`
- `GET /api/v1/app/customers/{id}/financial-overview`
- `GET /api/v1/app/order-lists`
- `POST /api/v1/app/orders/{id}/installment-payments`
- `POST /api/v1/app/installment-payments/sync`
- `POST /api/v1/admin/installment-payments`
- `PUT /api/v1/admin/installment-payments/{id}`
- `DELETE /api/v1/admin/installment-payments/{id}`

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `GET /api/v1/app/orders/{id}`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Removed: `status`
  - Added: `executionStatus` (number) — `0` NotStarted, `1` InProgress, `2` Completed
  - Added: `approvalStatus` (number) — `0` Pending, `1` Approved, `2` Rejected

### `GET /api/v1/admin/orders`
- **Type:** Modified
- **Request changes:**
  - Replace selected column key `Status` with `ExecutionStatus`
- **Response changes:**
  - Paginated row key `Status` → `ExecutionStatus`
  - Display label: **حالة التنفيذ** (Arabic string values: لم يبدأ بعد / جاري / مكتمل)

### `GET /api/v1/admin/orders/available-columns`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Column key `Status` → `ExecutionStatus`
  - Display name updated to **حالة التنفيذ**

### `GET /api/v1/admin/orders/excel-report`
- **Type:** Modified
- **Request changes:**
  - Replace column key `Status` with `ExecutionStatus` in `columns` query param
- **Response changes:**
  - Excel column header/key uses `ExecutionStatus` instead of `Status`

### `GET /api/v1/admin/orders/{id}/installment-payments`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Returns **empty list** when order `executionStatus` is `NotStarted` (previously when status was `PendingApproval`)

### `PUT /api/v1/admin/orders/{id}/seller-info`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - None (still `200 OK` on success)
  - On success, order is now **approved** (`approvalStatus: Approved`, `executionStatus: InProgress`)

### `GET /api/v1/admin/customers/{id}/orders`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Returns only orders with `approvalStatus: Approved` (same intent as excluding pending approval)

### `GET /api/v1/app/customers/{id}/orders`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Still returns only `executionStatus: InProgress` orders
  - App-created orders appear only after admin approval

### `GET /api/v1/app/customers/{id}/financial-overview`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Counts and totals include only `approvalStatus: Approved` orders
  - `activeOrdersCount` / `completedOrdersCount` use `executionStatus`

### `GET /api/v1/app/order-lists`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Aggregates (order counts, amounts, overdue) count only `executionStatus: InProgress` orders

### Installment payment write endpoints
- **Type:** Modified (`POST/PUT/DELETE` admin installment-payments, `POST` app order installment-payments, `POST` app installment-payments/sync)
- **Request changes:**
  - None
- **Response changes:**
  - Operations succeed only when order `executionStatus` is `InProgress`
  - `404` / order-not-found when order is `NotStarted` or not approved yet

---

## 4. Breaking Changes

- **`GET /api/v1/app/orders/{id}`:** `status` removed; use `executionStatus` and `approvalStatus`
- **Admin orders list/export:** column key `Status` renamed to `ExecutionStatus` — update saved column selections and UI mappings
- **Enum rename:** `PendingApproval` (`0`) → `NotStarted` under `executionStatus`; approval state is now a separate field
- **App order flow:** orders submitted via app stay `NotStarted` / `Pending` until admin completes `PUT /api/v1/admin/orders/{id}/seller-info`; installment collection and in-progress lists depend on this

---

## 5. Notes

- Enum values are returned as **numbers** (not strings)
- Map UI labels separately for `executionStatus` and `approvalStatus`; do not merge into a single status badge without checking both
- `overdueAmount` is `0` when `executionStatus` is `NotStarted` or `Completed`
- `PUT /api/v1/app/orders/{id}/seller-info` does **not** approve the order; only the admin seller-info endpoint does
- No new request body fields were added; clients only need to read and display the new response fields
