SELECT current_database();

INSERT INTO "Customers" (
  "FullName","MotherName","NationalCode","BirthDate",
  "PhoneNumber","WhatsAppPhoneNumber",
  "Business_Address","Business_Name","Business_NearestKnownLocation",
  "CreatedBy","LastUpdatedBy","BranchId"
) SELECT
  '04F Live Customer','N/A','N/A','1992-02-02',
  '07701112233','07701112233','addr','biz','near',1,1,1
WHERE NOT EXISTS (SELECT 1 FROM "Customers" WHERE "PhoneNumber"='07701112233');

WITH c AS (SELECT "Id" FROM "Customers" WHERE "PhoneNumber"='07701112233' ORDER BY "Id" DESC LIMIT 1)
INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId",
  "ExistingCustomerId","IsDeleted","ConcurrencyStamp","CreatedBy","CreatedAt"
) SELECT
  '04F Live Customer','07701112233',0,0,1,c."Id",false,0,1,NOW()
FROM c
RETURNING "Id";
