# إصدار لوحة الإدارة حتى الدفعة19

تم تفعيل API والواجهة في 2026-11-09 بتوقيت بغداد. لا تنفيذ للدفعة20 وما بعدها.

- API image: qalaat-api:20261109-admin19
- Panel image: qalaat-view:20261109-admin19-ui-fix
- Migration: 20261108213748_SalesEmployeeAssignmentAndLocationActor
- Server release: /opt/qalaat/releases/20261109-admin19
- Active compose overrides for API and panel: within the server release directory. Preserve them when deploying; do not run base compose alone.
- Protected backups: before-verify.dump and before-activation.dump on server only. Restored the first into an isolated verification database and successfully applied migrations before production activation.
- Clone: qalaat_verify_admin19_20261109b. Verification containers stopped.
- Existing row counts unchanged after activation: Users16, Customers3, Orders8, InstallmentPayments24.
- Backend tests305/305; frontend79/79; API publish and Vite build successful.

## Release fixes
- Updated stale assertions for implemented summary/declining features.
- InMemory cash-holder test now explicitly ignores unsupported transaction warning in test setup only. This test does not prove transaction behavior; no production transaction logic weakened.
- Admin template explicitly includes new SalesOverview, SalesEmployee, SalesLocation and SalesRoute read permissions. No changes to other roles for this fix.
- ManagementSummary details now requires both Read and Drilldown at API authorization.
- Corrected nested Vue Query ref access in summary page/dialog and ratings page after browser verification exposed false error states.

## Verification and limits
- Clone and production: normal login plus six API read smoke checks passed (SalesRequests counts, FollowerWork followers, SalesOverview, SalesEmployees, SalesRecords, ManagementSummary matrix).
- Production browser evidence is stored alongside this report. Normal login through the UI; no token files or production dumps exported.
- Initial activation restored previous images after an early panel readiness check. A retry with startup wait succeeded; additive schema retained.
- Existing incomplete business workflows from deliveries01–19 remain incomplete. Deployment is not proof of device GPS, all action flows, or completion of deferred legal intake and other documented gaps.
- No automatic SalesEmployeeAssignment backfill. Employee pages may be empty until real assignments are configured.
