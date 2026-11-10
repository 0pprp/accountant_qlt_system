WITH c AS (SELECT "Id" FROM "Customers" WHERE "PhoneNumber"='07701112233' ORDER BY "Id" DESC LIMIT 1)
INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId",
  "ExistingCustomerId","IsDeleted","ConcurrencyStamp","CreatedBy","CreatedAt"
) SELECT '04F Ex2','07701112233',0,0,1,c."Id",false,0,1,NOW() FROM c
RETURNING "Id";
