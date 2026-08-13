# API Change Log — Customer Financial Overview

## 1. Summary

- Added a new **App** endpoint to return an aggregated financial overview for a single customer across all accessible orders.

---

## 2. API Changes

### New Endpoints
- `GET /api/v1/app/customers/{id}/financial-overview`

### Modified Endpoints
- None

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `GET /api/v1/app/customers/{id}/financial-overview`
- **Type:** New
- **Permission:** `Customer.Read`
- **Request changes:**
  - Path parameter: `id` (customer ID, integer)
  - No query parameters or request body
- **Response changes:**
  - **200 OK** — aggregated financial summary:
    - `customerId` (number) — customer ID
    - `totalPaidAmount` (number) — total amount paid across all included orders
    - `totalOverdueAmount` (number) — total overdue amount across active orders
    - `totalSellAmount` (number) — total sell amount the customer must pay
    - `totalRemainingAmount` (number) — remaining balance (`totalSellAmount - totalPaidAmount`)
    - `totalDailyInstallmentAmount` (number) — sum of daily installments for in-progress orders
    - `activeOrdersCount` (number) — count of in-progress orders
    - `completedOrdersCount` (number) — count of completed orders
  - **404 Not Found** — customer not found (`Customer_Not_Found`)

---

## 4. Breaking Changes

- None

---

## 5. Notes

- App area only (`/api/v1/app/...`)
- Data is scoped to orders where the current user is assigned as Mandob or Motaba on the order list
- Pending-approval orders are excluded from financial calculations
- Amount fields are numeric; handle formatting on the client
