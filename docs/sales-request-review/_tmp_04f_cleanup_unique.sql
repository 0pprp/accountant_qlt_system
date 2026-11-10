SELECT current_database();
UPDATE "SalesRequests" SET "LinkedOrderId" = NULL WHERE "LinkedOrderId" IS NOT NULL;
DELETE FROM "Orders" WHERE "Id" = 1;
DELETE FROM "Customers" WHERE "FullName" = '04F Unique Probe';
SELECT 'cleaned' AS status;
