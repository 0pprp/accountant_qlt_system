# API Change Log — Users Dynamic Columns & Orders List Updates

## 1. Summary

- Admin **users list** now supports **dynamic column selection**, aligned with the admin orders list pattern.
- Clients can fetch selectable user columns from a new endpoint and pass chosen column keys when loading the paginated list.
- Admin **orders list** adds optional **date-range filters** and a new **`totalBuyAmount`** summary field in the response.

---

## 2. API Changes

### New Endpoints
- `GET /api/v1/admin/users/available-columns`

### Modified Endpoints
- `GET /api/v1/admin/users`
- `GET /api/v1/admin/orders`

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `GET /api/v1/admin/users/available-columns`
- **Type:** New
- **Request changes:**
  - None
- **Response changes:**
  - Returns an array of column definitions: `key`, `displayName`, `dataType`
  - Available keys: `Id`, `FullName`, `MotherName`, `Username`, `NationalCode`, `BirthDate`, `PhoneNumber`, `Address`, `BranchNames`, `RoleNames`, `CreationStep`, `UndeliveredCashAmount`, `SalaryType`, `SalaryAmount`, `SaleSharePercent`, `InstallmentSharePercent`, `CreatedAt`

### `GET /api/v1/admin/users`
- **Type:** Modified
- **Request changes:**
  - Added optional query param: `columns` (repeatable) — column keys from `available-columns`
  - Existing filters unchanged: `branchId`, `roleIds`, `searchTerm`, `startDate`, `endDate`, pagination, `sortCriteria`
  - If `columns` is omitted, all available columns are returned
- **Response changes:**
  - Root shape is now `{ paginatedUsers: { items, pageIndex, pageSize, totalCount } }`
  - Each item is a **dynamic object** keyed by selected column names (e.g. `Id`, `FullName`, `BranchNames`)
  - `BranchNames` and `RoleNames` are comma-separated strings (not arrays)
  - Enum-like fields (`CreationStep`, `SalaryType`) are returned as **Arabic labels** (strings)

### `GET /api/v1/admin/orders`
- **Type:** Modified
- **Request changes:**
  - Added optional query filters: `startDate`, `endDate` (`DateTime`) — filter orders by `createdAt` range
  - Existing filters unchanged: `branchId`, `searchTerm`, `customerId`, `approvalStatus`, `columns`, pagination, `sortCriteria`
- **Response changes:**
  - Added root-level field: `totalBuyAmount` (number) — sum of `buyAmount` for the filtered result set
  - Existing `totalSellAmount` and `paginatedOrders` unchanged

---

## 4. Breaking Changes

### Users
- **Response shape:** list data moved from root-level `items` to `paginatedUsers.items`
- **Row shape:** fixed fields (`id`, `fullName`, `branchNames`, `createdAt`) replaced by dynamic keys matching selected columns (PascalCase keys, e.g. `Id`, `FullName`)
- **`branchNames`:** no longer an array; use `BranchNames` as a comma-separated string when that column is selected

### Orders
- None

---

## 5. Notes

### Users
- Requires permission `User.Read`
- Column keys are **case-insensitive** in the `columns` query param
- Invalid or unknown column keys are ignored
- `sortCriteria` still applies to underlying user entity property names (e.g. `FullName`, `CreatedAt`), not computed columns like `BranchNames`
- Same dynamic-column pattern as `GET /api/v1/admin/orders` — reuse existing table/column-picker UI where possible

### Orders
- Requires permission `Order.Read`
- `startDate` / `endDate` filter on `createdAt` (inclusive)
- `totalBuyAmount` and `totalSellAmount` reflect the **full filtered dataset**, not just the current page

---

## 6. Changed Areas (Git Scope)

- **Users admin list:** dynamic columns feature — controller, paginated query/handler/response, column registry, available-columns query
- **Orders admin list:** date-range filtering and `totalBuyAmount` summary — paginated query filter, handler, response
- **No removed or deprecated endpoints** in this change set
