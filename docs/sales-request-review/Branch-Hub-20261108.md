# Branch filter and Hub — 2026-11-08

- Province UI now means authorized account branches; choices come from auth.branches even when request count is zero.
- Filter sends branchId, clears obsolete provinceId and hidden filters, resets paging on filter change, and preserves selection on reload.
- All means all authorized branches; server scope enforcement is unchanged.
- SignalR uses withCredentials:false with JWT. No CORS origins were broadened.
- Nginx /hubs/ now upgrades WebSocket and disables buffering. Hub access logs disabled to avoid recording query access tokens. Backup: /opt/qalaat/releases/20261107-sales-review/nginx-api-before-hub.conf.
- Live browser verified with the explicitly authorized ahmed account: Connected, 12 branch choices, branchId query, reload preservation, reset to all. No production business data was created or altered. Event delivery latency was not remeasured in this patch.
- Frontend build passed; SalesRequests 14/14 tests passed. Global typecheck retains 8 unrelated errors.
- Active image: qalaat-view:20261108-branch-hub. Release: /opt/qalaat/releases/20261108-branch-hub. Previous image retained as qalaat-view:pre-20261108-hub.
- Compose override remains /opt/qalaat/releases/20261107-sales-review/qalaat_view-active-override.json with updated image. API and database unchanged.
