SELECT current_database();
UPDATE "Users" SET "CreationStep"=3 WHERE "Username" IN ('emp_a_04f','emp_b_04f');

WITH c AS (
  SELECT "Id" FROM "Customers" WHERE "PhoneNumber"='07701112233' ORDER BY "Id" DESC LIMIT 1
)
INSERT INTO "SalesRequests" (
  "CustomerName","CustomerPhone","SourceType","Status","BranchId",
  "ExistingCustomerId","IsDeleted","ConcurrencyStamp","CreatedBy","CreatedAt"
) SELECT
  '04H Web Cycle Customer — اسم طويل للمراجعة',
  '07701112233',
  1, -- EmployeeSubmitted
  0,
  1,
  c."Id",
  false,
  0,
  1,
  NOW()
FROM c
RETURNING "Id","CustomerName","Status","SourceType";
