SELECT current_database();
UPDATE "Orders" SET "SellerId"=4 WHERE "Id"=5;
SELECT "Id","SellerId" FROM "Orders" WHERE "Id"=5;
SELECT "Id","Status","LinkedOrderId","TargetEmployeeId" FROM "SalesRequests" WHERE "Id"=7;
