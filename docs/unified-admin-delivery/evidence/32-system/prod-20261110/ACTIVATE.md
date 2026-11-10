# Production activate 20261110-web-update

- Host: 161.97.83.160 (panel/api.qalataldhaman.com)
- Tag: 20261110-web-update
- DB backup: /opt/qalaat/releases/20261110-web-update/before-activation.dump (212K)
- Previous images: qalaat-api:20261109-admin19, qalaat-view:20261109-management-split-mobile
- Active: qalaat-api:20261110-web-update, qalaat-view:20261110-web-update
- Smoke:
  - https://panel.qalataldhaman.com/auth -> 200
  - POST /api/v1/admin/Auth/login (bad creds) -> 400
  - GET /api/v1/app/mobile-updates/delegate -> 200 (seeded defaults, forceUpdate=false)
- Rollback: restore overrides under /opt/qalaat/releases/20261110-web-update/previous-*-override.json and recreate containers; DB dump available for schema/data restore if needed.
