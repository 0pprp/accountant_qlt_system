# Evidence 21 — Sales Legal Followup

- DB isolated: `qalaat_20_21_iso`
- API-live: PASS — create/list/cancel/idempotent; request→New; customer retained; outbox StatusChanged
- Browser-live: PASS — `/sale/legal` shows created InProgress case
- Screenshots: `legal-360.png`, `legal-768.png`, `legal-1440.png`
- SQL checks: `verify_effects.sql`
- Shared API log: `../20-21-live-api.json`
- Missing: case document file preview storage
