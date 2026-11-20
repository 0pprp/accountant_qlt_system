# المروّج الميداني — عقد الـ API

All JSON uses camelCase. Errors: HTTP 400/403/409 with body `{ errors: [{ message, code }] }` (message is Arabic, show it as-is).
Auth: `Authorization: Bearer <accessToken>` from `POST api/v1/app/Auth/login {username,password}` → `{accessToken, refreshToken, permissions, branches, roles}`;
refresh `POST api/v1/app/Auth/refresh-token {refreshToken}` (old token stays valid 24h, so retrying a lost refresh is safe).
Mobile update check: `GET api/v1/app/mobile-updates/field-promoter?currentVersionCode=N` (anonymous).
Times are UTC ISO-8601; the day boundary is Iraq time (UTC+3).

## App — `api/v1/app/field-promoter` (permission `FieldPromoter.App`)

| Method | Route | Body / query | Returns |
|---|---|---|---|
| GET | `me` | – | `{id, fullName, username, branches:[{id,name}], isWorking, shiftStartedAt, todayPins, todayOrders, todayWorkMinutes, targetMinutes, monthOrders, monthSold, supportPhone, supportWhatsapp, products:[string]}` |
| POST | `shift/start` | – | `{isWorking, startedAt, endedAt}` (also auto-starts on first order/pin) |
| POST | `shift/end` | – | same |
| GET | `today` | – | `{isWorking, startedAt, sessions:[{startedAt,endedAt}], periods:[{start,end,minutes,events}], workMinutes, targetMinutes, pins, orders, route:[{lat,lng,at}]}` |
| POST | `locations/batch` | JSON array `[{lat, lng, accuracy, recordedAt, clientEventId}]` max 500, clientEventId required (≤64 chars, unique per point) | `{accepted, duplicate, rejected, isWorking}` — if `isWorking=false` stop tracking. Remove from local queue every point that was accepted OR duplicate; drop rejected ones too (they will never be accepted). |
| POST | `orders` | **multipart/form-data**: `submissionKey` (uuid, one per submit attempt, reuse on retry), `branchId`, `customerName` (3 Arabic names), `customerPhone` (07XXXXXXXXX, +964 accepted), `product` (one of me.products or free text 2–100), `address` (3–400), `note?` (≤500), `latitude?`, `longitude?` (both or neither, inside Iraq), `photo?` (JPEG/PNG ≤2MB, image/jpeg content type) | `{id, alreadySubmitted}`; 409-like 400 code `FieldPromoter.DuplicateOpenOrder` when an open order exists for same phone+product |
| GET | `orders?tab=inProgress|sold|finished|rejected|all&page=1` | – | `{items:[Order], page, hasMore}` (20/page) |
| GET | `orders/counts` | – | `{inProgress, sold, finished, rejected}` |
| GET | `orders/{id}` | – | Order |
| POST | `orders/{id}/cancel` | `{reason?}` | 200; only when `canCancel` |
| GET | `pins?minLat&minLng&maxLat&maxLng` | box (server caps to ±0.5°, 500 pins) | `[{id, lat, lng, name, interest, mine}]` |
| GET | `pins/mine?from=yyyy-MM-dd&to=yyyy-MM-dd&q=&page=1` | – | `{items:[Pin], total, page, hasMore}` (30/page) |
| GET | `pins/{id}` | – | Pin (phone/note null unless mine) |
| POST | `pins` | `{lat, lng, name (2–150), interest 1–5, phone?, note? (≤500), agentLat?, agentLng?}` | Pin |
| PUT | `pins/{id}` | same (lat/lng ignored) | Pin (owner only) |
| DELETE | `pins/{id}` | – | 200 (owner only) |

Order = `{id, customerName, customerPhone, product, address, note, branchName, stage: "inProgress"|"sold"|"finished"|"rejected", statusLabel, reason, createdAt, canCancel, sale: null | {total, paid, remaining, paymentsCount, lastPaymentDate, percent}}`
Pin = `{id, lat, lng, name, interest, interestLabel, phone, note, createdAt, updatedAt, mine}`
Interest labels 1..5: غير مهتم إطلاقاً، قليل الاهتمام، مهتم نوعاً ما، مهتم، مهتم جداً. Colors: #C62828 #EF6C00 #F9A825 #7CB342 #2E7D32.

## Panel — `api/v1/admin/field-promoters`

Permissions: `FieldPromoter.Read` (view), `FieldPromoter.Manage` (accounts, settings, delete pin), `FieldPromoter.Alerts`.
Every list is limited to promoters sharing a branch with the signed-in user; `branchId` narrows further.

| Method | Route | Returns |
|---|---|---|
| GET | `live?branchId` | `{total, working, stationary, notStarted, pinsToday, interestedPinsToday, ordersToday, soldToday, unseenAlerts, metYesterday, targetedYesterday, promoters:[{id, fullName, phone, status: working|stationary|notStarted|ended|suspended, statusLabel, lat, lng, lastAt, shiftStartedAt, workMinutes, targetMinutes, pins, orders, targetPercent, isSuspended}]}` — poll every 15–30s |
| GET | `{userId}/day?date=yyyy-MM-dd` | `{userId, fullName, date, firstShiftStart, workMinutes, targetMinutes, targetStatus, pins, orders, sessions, periods, route:[{lat,lng,at}], pinList:[{id,lat,lng,name,interest,interestLabel,phone,note,at,minutesSincePrevious,quick}], orderList:[{id,customerName,product,statusLabel,at}], alerts:[{id,lat,lng,since,lastSeen,resolvedAt,minutes}]}` |
| GET | `attendance?from&to&branchId` (≤93 days) | `{from,to,days:[date], rows:[{userId, fullName, targetMinutes, met, missed, off, days:[{day, status: Met|Missed|Off|NoTarget|Today, label, workMinutes, targetMinutes, pins, orders}]}]}` |
| GET | `attendance/export?from&to&branchId` | xlsx file |
| GET | `alerts?openOnly&page` | `{items:[{id,userId,fullName,lat,lng,since,lastSeen,resolvedAt,minutes,seen}], unseen, page, hasMore}` |
| POST | `alerts/{id}/seen`, `alerts/seen-all` | 200 |
| GET | `pins?branchId&userId&minInterest&from&to&q&page` | `{items:[{id,lat,lng,name,interest,interestLabel,phone,note,userId,promoterName,createdAt}], total, page, hasMore}` (200/page) |
| DELETE | `pins/{id}` | 200 (Manage) |
| GET | `orders?branchId&userId&stage&from&to&page` | `{items:[{order: Order, userId, promoterName, hasShopPhoto, shopLat, shopLng}], total, page, hasMore, counts:{inProgress,sold,finished,rejected}}` |
| GET | `orders/{id}/shop-photo` | image (fetch as blob with token) |
| GET | `promoters?q&status=active|suspended&branchId` | `[{id, fullName, username, phone, branches:[{id,name}], isSuspended, suspendReason, targetMinutes}]` |
| POST | `promoters` | body `{fullName, username, password (≥8), phone?, branchIds:[int]}` → id |
| PUT | `promoters/{id}` | body `{fullName, username, phone?, branchIds}` → id |
| POST | `promoters/{id}/password` | `{password}` (≥8; signs the promoter out everywhere) |
| POST | `promoters/{id}/suspend` | `{suspended: bool, reason?}` (suspend = immediate sign-out + ends open shift) |
| PUT | `promoters/{id}/target` | `{minutes: int|null}` (null = default) |
| GET/PUT | `settings` | `{defaultDailyWorkMinutes (0–1440), stationaryRadiusMeters (30–2000), stationaryMinutes (5–600), supportPhone, supportWhatsapp, locationRetentionDays}` |

Sale requests from promoters also appear in the normal «طلبات البيع» boxes with source label «المروّج الميداني» (SourceType = 6).
