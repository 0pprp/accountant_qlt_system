SELECT "Id", "Status", "IsDeleted", "TargetEmployeeId", "CustomerName"
FROM "SalesRequests" WHERE "Id" = 1;

SELECT "Id", "Status", "RowVersion", "SalesRequestId"
FROM "LegalFollowupCases" WHERE "Id" = 1;

SELECT COUNT(*) AS customer_still_there FROM "Customers" WHERE "PhoneNumber" = '07709998887';

SELECT "ChangeType", "RequestId", "PublishedAtUtc" IS NOT NULL AS published
FROM "SalesRequestChangeOutbox"
WHERE "RequestId" = 1
ORDER BY "Id";

SELECT "EventType", "FromStatus", "ToStatus", "Note"
FROM "SalesRequestTimelineEntries"
WHERE "SalesRequestId" = 1
ORDER BY "Id";
