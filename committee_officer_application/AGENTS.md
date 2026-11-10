# Agent notes — مسؤول اللجنة

- Product label: **مسؤول اللجنة** (دور النظام: Motaba).
- UI kit from `sales_employee_app` (waves, glassy bottom bar, centered dialogs) — not follower UI fork. **Do not redesign** when porting actions.
- Business **actions** from `follower_application` (SalesHaider) — reference only; **APIs are this system's** `api.qalataldhaman.com` / `api/v1/app/committee`.
- **Never** call `admincompany.alsaaeidy.com` / SalesHaider Followers APIs from this app.
- `GATEWAY_BASE` defaults empty; set only to قلعة الضمان gateway if/when tasks exist.
- Safe Scaffold: always respect `MediaQuery.paddingOf(context).bottom`.
- Dialogs: `core/widgets/centered_dialog.dart` only.
- Docs: `docs/generate_docs.py` → Word + Excel (paths, architecture, endpoints, SystemActions).
