# API Change Log — Order Approval Workflow

## 1. Summary

- Admin can **approve or reject** app-submitted orders via a dedicated endpoint after the seller completes the order flow.
- App users can **delete** their own incomplete orders.
- App order list now returns **`step`** and **`approvalStatus`**.
- Admin order list/export supports **`approvalStatus`** filter and column.
- **`PUT /api/v1/app/orders/{id}/seller-info`** no longer activates the order; financial side-effects run only on admin approval.
- **Admin-created** orders start as **`approvalStatus: Approved`** (previously `Pending`).

---

## 2. API Changes

### New Endpoints
- `PATCH /api/v1/admin/orders/{id}/approval-status`
- `DELETE /api/v1/app/orders/{id}`

### Modified Endpoints
- `GET /api/v1/app/orders`
- `PUT /api/v1/app/orders/{id}/seller-info`
- `GET /api/v1/admin/orders`
- `GET /api/v1/admin/orders/available-columns`
- `GET /api/v1/admin/orders/excel-report`

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `PATCH /api/v1/admin/orders/{id}/approval-status`
- **Type:** New
- **Request changes:**
  - Body: `{ "approvalStatus": number }`
  - Allowed values: `1` (Approved), `2` (Rejected) — `0` (Pending) is rejected by validation
- **Response changes:**
  - `200 OK` on success (empty body)
  - Possible error codes: `Order_Not_Found`, `Order_Information_Has_Not_Completed_Yet`, `ApprovalStatus_Of_Order_Has_Been_Set_Before`

### `DELETE /api/v1/app/orders/{id}`
- **Type:** New
- **Request changes:**
  - None (route `id` only)
- **Response changes:**
  - `200 OK` on success (empty body)
  - Possible error codes: `Order_Not_Found`, `Order_Does_Not_Belong_To_You`, `You_Can_Not_Delete_This_Order`
  - Deletion allowed only when order belongs to current user, `step` is not `Completed`, and order has **no** installment payments

### `GET /api/v1/app/orders`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Added per item: `step` (number) — `0` Attachments, `1` SellerInfo, `2` Completed
  - Added per item: `approvalStatus` (number) — `0` Pending, `1` Approved, `2` Rejected

### `PUT /api/v1/app/orders/{id}/seller-info`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Still `200 OK` on success
  - On success, order moves to `step: Completed` but stays `approvalStatus: Pending` and `executionStatus: NotStarted`
  - Prepayment installment and foreign-product transactions are **no longer created** by this call

### `GET /api/v1/admin/orders`
- **Type:** Modified
- **Request changes:**
  - Added optional query filter: `approvalStatus` (`0` | `1` | `2`)
  - New selectable/export column key: `ApprovalStatus`
- **Response changes:**
  - When `ApprovalStatus` column is selected, row value is Arabic label: قيد المراجعة / تمت الموافقة / مرفوض

### `GET /api/v1/admin/orders/available-columns`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Added column key `ApprovalStatus` — display name **حالة الموافقة**

### `GET /api/v1/admin/orders/excel-report`
- **Type:** Modified
- **Request changes:**
  - Supports `approvalStatus` filter (same as list)
  - Supports `ApprovalStatus` in `columns` query param
- **Response changes:**
  - Excel export can include approval status column

---

## 4. Breaking Changes

- **App order activation:** completing `PUT /api/v1/app/orders/{id}/seller-info` no longer makes the order collectible (`executionStatus` stays `NotStarted` until admin approval)
- **Admin approval action:** use `PATCH /api/v1/admin/orders/{id}/approval-status` for app-submitted orders waiting review — do not expect seller-info completion alone to approve them
- **Admin-created orders:** `POST /api/v1/admin/orders` now creates orders with `approvalStatus: Approved` instead of `Pending`

---

## 5. Notes

- Enum values are returned as **numbers** (not strings)
- Admin approval requires order `step: Completed` and `approvalStatus: Pending`; can only be set once
- On admin **approve**, order becomes `approvalStatus: Approved` and `executionStatus: InProgress`; prepayment and foreign-product side-effects are applied at that point
- On admin **reject**, order becomes `approvalStatus: Rejected`; `executionStatus` stays `NotStarted`
- Show a delete action in the app only for the seller's own orders that are not completed and have no installments
- Requires permission `Order.Update` (admin patch) and `Order.Delete` (app delete)
