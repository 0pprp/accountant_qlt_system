# API Change Log — Order Foreign Product Purchase Sync

## 1. Summary

- **`TransactionType`:** removed value `4` (`ForeignProductOrder`).
- **Admin purchase APIs:** purchase line items can represent foreign products (`foreignProductName`, nullable `product` / `productId`).

---

## 2. API Changes

### New Endpoints
- None

### Modified Endpoints
- `GET /api/v1/admin/purchases`
- `GET /api/v1/admin/purchases/{id}`
- `PUT /api/v1/admin/purchases/{id}`

### Removed Endpoints
- None

### Deprecated Endpoints
- None

---

## 3. Endpoint Details

### `GET /api/v1/admin/purchases`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - In each `purchaseItems[]` item:
    - `product` — was required; now **nullable**
    - `foreignProductName` — **added** (optional string)

### `GET /api/v1/admin/purchases/{id}`
- **Type:** Modified
- **Request changes:**
  - None
- **Response changes:**
  - Same as paginated list for `purchaseItems[]` (`product` nullable, `foreignProductName` added)

### `PUT /api/v1/admin/purchases/{id}`
- **Type:** Modified
- **Request changes:**
  - In each `purchaseItems[]` item:
    - `productId` — was required; now **optional** (`int?`)
    - `foreignProductName` — **added** (optional string, max length 200 when provided)
- **Response changes:**
  - None

---

## 4. Breaking Changes

- **`TransactionType`:** do not use or display **`4` (`ForeignProductOrder`)**; it will not appear on new data. Remaining values: `0` SellerPayment, `1` Purchase, `2` Expense, `3` SafeTransfer.
- **Purchase list/detail:** clients must handle **`product: null`** and read **`foreignProductName`** for foreign lines.
- **Purchase update:** clients must not assume **`productId`** is always required on every line item; foreign lines use **`foreignProductName`**.

---

## 5. Notes

- None
