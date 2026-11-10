# Mobile and panel workflow parity implementation

Date: 2026-11-09. Scope: reviewed source flows and the corrections below. This is not a declaration of full application parity or production readiness. No production deployment, database modification, or migration was performed in this change.

## Source decisions

- Old delegate_application/lib/delegate_sales_requests_page.dart and follower_application/lib/follower_sales_requests_page.dart are originator inboxes, not employee assignment bins. Four filters: all, in progress, rejected, completed.
- Old BE_DelegateWebApplication/BE_DelegateWebApplication/Repository/FollowerActionsRepository.cs ListSalesRequestsForDelegateAsync defines deletion visibility: incomplete removed requests appear rejected; completed removed requests are hidden.
- Old follower_application/lib/HomePage.dart _addListDelegateNote writes a note about the selected list/delegate. It is not a customer note or a message to management.
- Old sales_employee_application/lib/screens/pending_sales_screen.dart requires a preparation note. Cancelling the note prompt must not prepare a request.

## Endpoint and authorization

GET /api/v1/app/SalesRequests/submitted/{source}?tab=all&page=1

Supported sources use the existing submission permission mapping (Delegate, Follower, EmployeeSubmitted). Caller must possess the corresponding Submit permission to read their submission receipts. AppRead for assigned employee work does not substitute for it.

The handler intersects source, CreatedByUserId from session, branch claims, and current database UserBranches membership. It accepts no client author id. Allowed tabs: all, inProgress, rejected, completed. Page 1..10000, fixed 30 rows with one extra row for hasMore. Projection and AsNoTracking avoid loading unrelated entities. Sorting: CreatedAt descending, Id descending.

Response: items, page, hasMore. Each item: requestId, customerName, customerPhone, address, requestedProduct, submissionNote, statusLabel, targetEmployeeName, rejectionReason, pendingNote, createdAt, branchName.

POST /api/v1/app/committee/order-lists/{listId}/delegate-notes

Existing endpoint now wired to the committee home list card. Payload: noteText only. Existing server permission FollowerDelegateNotes.Write (resolve actual seed constant FollowerWork.DelegateNotesWrite), actor from session, current Motaba assignment required. The button prevents repeated activation while saving and displays success only after the API succeeds. Cancellation makes no request.

## File responsibilities

- api-main/api-main/src/Application/SalesRequests/Queries/App/Submitted/GetSubmittedSalesRequestsQuery.cs: request and projected response contracts.
- .../GetSubmittedSalesRequestsHandler.cs: permission, author/source/branch scope, bounded query and legacy state mapping.
- api-main/api-main/src/Api/Controllers/V1/AppArea/SalesRequestsController.cs: route forwarding only.
- delegate_app and committee_officer_application lib/features/sales/data/submitted_requests_remote.dart: typed inbox contract and cancellable request.
- Both apps lib/features/sales/presentation/submitted_sales_requests_page.dart: four filters, complete card fields, pagination, retry, new request entry and stale response exclusion.
- delegate_app/lib/features/home/presentation/widgets/home_drawer.dart: inbox entry and scrollable navigation.
- committee_officer_application/lib/features/sales/presentation/sales_requests_page.dart: inbox wrapper instead of mistakenly opening create-only form.
- committee_officer_application/lib/features/home/data/committee_remote.dart: delegate note transport.
- .../presentation/widgets/delegate_note_button.dart: selected-list note action and centered system dialog.
- .../presentation/home_tab.dart: action attached to its list card.
- .../presentation/tutorial_videos_page.dart: nullable error compile correction.
- sales_employee_app/lib/features/sales/presentation/widgets/sales_request_card.dart: visible customer/contact/address/product/source/notes/timestamps and existing detail entry.
- .../sales_requests_bin_page.dart: uses reusable card and checks mounted after returning from detail.
- .../sales_request_detail_page.dart and domain/sales_request_actions.dart: mandatory preparation note; cancellation causes no mutation; text draft avoids controller disposal during dialog exit.
- .../complete_sale_wizard_page.dart: no fictitious initial customer name, phone, province on the real sale path.
- panel-admin-main/panel-admin-main/src/modules/SalesRequests/components/RequestCard.vue: Baghdad timestamp and semantic dialog state.
- .../views/SalesRequestsWorkspace.vue: reflects the selected outer matching card.

## Verification

- API SalesRequests tests: 104 passed (includes 8 new originator scope/filter/paging cases; uses EF InMemory, not PostgreSQL live proof).
- API build: succeeded with 0 errors.
- Delegate full Flutter suite: 12 passed. A stale smoke expectation accepted 100 despite the existing central payment validator requiring multiples of 1000; corrected to reject 100 and accept 1000 without changing the payment rule.
- Sales employee full Flutter suite: 39 passed, including 360-wide complete card and detail action.
- Committee full Flutter suite: 18 passed before adding an additional save-path case; focused note tests subsequently 2 passed.
- Delegate and committee inbox tests exercise stale out-of-order responses, error versus empty, and retry.
- Panel SalesRequests tests: 15 passed; vue-tsc --noEmit succeeded.
- Flutter analyzer: delegate, committee, sales have no fatal errors with --no-fatal-infos --no-fatal-warnings; existing infos/warnings remain. This is not a warning-free claim.
- Flutter devices: Windows, Chrome, Edge available; no Android device detected during this session.

## Remaining work

- API-live against isolated PostgreSQL for the new submitted endpoint and the newly wired committee note; browser/device end-to-end verification.
- Full old/new screen-by-screen parity remains unclosed. Sales drawer archive/tutorials/complaint/manager note and committee statistics still need source and API review.
- Current employee inbox read handlers should be audited consistently with current branch/assignment revocation; this change does not claim to close that cross-cutting review.
- Old existing-customer request linkage is not introduced here. Current create contract remains the agreed name, phone, address, product and optional note.
- Existing rollout backlog remains authoritative for all other unfinished features; no items are silently dropped.
- Word/Excel delivery artifacts have not yet been regenerated for this increment. This markdown records the exact scope and evidence without overstating live verification.
