# Evidence 22 — Sales Request Integration

- DB isolated: `qalaat_22_23_iso` (template from `qalaat_20_21_iso`, not production)
- Unit: SalesRequestSubmission + LegalFollowupCompletion + Flutter form tests (3 apps) PASS
- API-live: see `live-api.json` when run against http://127.0.0.1:4200
- Device-verified: Missing in this session (no physical device logged)
- Closed from 21: field complete + document preview (Admin/App download + panel UI)

## Notes
- Product contract: text `RequestedProduct` (not silent ProductId swap)
- Source from route; CreatedBy from session; Branch from user branches
- Idempotency: SubmissionKey + fingerprint + lock
