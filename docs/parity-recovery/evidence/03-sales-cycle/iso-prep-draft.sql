INSERT INTO "Customers" (
  "FullName","MotherName","NationalCode","BirthDate","PhoneNumber","WhatsAppPhoneNumber","BranchId",
  "Business_Name","Business_Address","Business_NearestKnownLocation",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy"
)
SELECT 'زبون مسودة','أم','NC-DRAFT-1','1990-01-01','07806667788','07806667788',1,
       'محل','عنوان','علامة', NOW(),1,NOW(),1
WHERE NOT EXISTS (SELECT 1 FROM "Customers" WHERE "NationalCode"='NC-DRAFT-1');

UPDATE "SalesRequests"
SET "ExistingCustomerId" = (SELECT "Id" FROM "Customers" WHERE "NationalCode"='NC-DRAFT-1' LIMIT 1),
    "TargetEmployeeId" = 3,
    "Status" = 1,
    "LinkedOrderId" = NULL
WHERE "Id" = 11;

SELECT "Id","TargetEmployeeId","ExistingCustomerId","LinkedOrderId","Status"
FROM "SalesRequests" WHERE "Id"=11;
