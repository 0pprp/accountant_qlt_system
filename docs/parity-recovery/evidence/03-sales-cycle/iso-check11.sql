SELECT "Id","LinkedOrderId","TargetEmployeeId" FROM "SalesRequests" WHERE "Id"=11;
SELECT COUNT(*) AS order_rows FROM "Orders" WHERE "Id" = (SELECT "LinkedOrderId" FROM "SalesRequests" WHERE "Id"=11);
