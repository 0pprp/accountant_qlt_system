UPDATE "SalesRequests"
SET "ExistingCustomerId"=(SELECT "Id" FROM "Customers" WHERE "NationalCode"='NC-DRAFT-1' LIMIT 1),
    "TargetEmployeeId"=3, "Status"=1, "LinkedOrderId"=NULL
WHERE "Id"=14;
