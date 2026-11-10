# 04H Flutter targets (updated)

## flutter devices
Windows / Chrome / Edge available. Android: none. Windows desktop: VS C++ missing (not installed).

## blockers
- Android: **Mobile-blocked** — no device / cmdline-tools
- Windows desktop: Live-blocked — VS Desktop C++ not installed (per 04H)
- GPS: duty uses `LocationService.webFallback` — **not location-proven**

## Results (separate)
- **Web shell verified**: login, duty gate, home counts via Dio after SessionStore Web fix, bottom-nav + home bins → `SalesRequestsBinPage` (`/sales-requests/...`), drawer keeps Orders as «سجلات البيع»
- **Sales lifecycle verified** (Flutter Web client, not raw API for employee actions):
  - Request **#13**: view → prepare → inspect → complete-sale wizard → تم البيع (order #8)
  - Panel sold tab shows #13; Hub status متصل
  - Request **#14**: panel reassign A→B; emp_a inbox/bins no longer show #14
  - Offline open bin → error dialog + «إعادة المحاولة» (no fake data)

## Dio root causes fixed
1. Before-send: secure-storage DomException on Web → SessionStore memory+SharedPreferences
2. Attachments: form field `Type` + `MultipartFile.fromBytes` on Web
3. Mandob: do not send `orderListId` on seller-info
