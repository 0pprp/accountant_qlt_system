-- Isolated verification data for batches 20+21 (qalaat_20_21_iso only). Not production.

INSERT INTO "Provinces" ("Name")
SELECT 'بغداد-تحقق'
WHERE NOT EXISTS (SELECT 1 FROM "Provinces" WHERE "Name" = 'بغداد-تحقق');

INSERT INTO "Branches" ("Name", "ProvinceId", "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy")
SELECT 'فرع تحقق 20-21', p."Id", NOW(), 1, NOW(), 1
FROM "Provinces" p
WHERE p."Name" = 'بغداد-تحقق'
  AND NOT EXISTS (SELECT 1 FROM "Branches" WHERE "Name" = 'فرع تحقق 20-21');

INSERT INTO "Safes" ("Name", "RemainingCashAmount", "CreditAmount", "DebitAmount", "BranchId", "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy")
SELECT 'قاصة فرع تحقق', 0, 0, 0, b."Id", NOW(), 1, NOW(), 1
FROM "Branches" b
WHERE b."Name" = 'فرع تحقق 20-21'
  AND NOT EXISTS (SELECT 1 FROM "Safes" WHERE "BranchId" = b."Id");

INSERT INTO "UserBranches" ("UserId", "BranchId")
SELECT 1, b."Id"
FROM "Branches" b
WHERE b."Name" = 'فرع تحقق 20-21'
  AND NOT EXISTS (
    SELECT 1 FROM "UserBranches" ub WHERE ub."UserId" = 1 AND ub."BranchId" = b."Id"
  );

INSERT INTO "Users" (
  "FullName", "MotherName", "Username", "PasswordHash", "NationalCode", "BirthDate",
  "PhoneNumber", "SecurityStamp", "CreationStep", "CreatedAt", "CreatedBy",
  "LastUpdatedAt", "LastUpdatedBy", "Address", "UndeliveredCashAmount", "PermissionsVersion"
)
SELECT
  'متابع تحقق', 'أم', 'follower_iso',
  (SELECT "PasswordHash" FROM "Users" WHERE "Id" = 1),
  'ISO-FOLLOW', DATE '1990-01-01',
  '07000000001', gen_random_uuid(), 2, NOW(), 1, NOW(), 1, 'عنوان', 0, 0
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'follower_iso');

INSERT INTO "UserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "Users" u
CROSS JOIN "Roles" r
WHERE u."Username" = 'follower_iso' AND r."Name" = 'Motaba'
  AND NOT EXISTS (
    SELECT 1 FROM "UserRoles" ur WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
  );

INSERT INTO "UserBranches" ("UserId", "BranchId")
SELECT u."Id", b."Id"
FROM "Users" u
CROSS JOIN "Branches" b
WHERE u."Username" = 'follower_iso' AND b."Name" = 'فرع تحقق 20-21'
  AND NOT EXISTS (
    SELECT 1 FROM "UserBranches" ub WHERE ub."UserId" = u."Id" AND ub."BranchId" = b."Id"
  );

INSERT INTO "Users" (
  "FullName", "MotherName", "Username", "PasswordHash", "NationalCode", "BirthDate",
  "PhoneNumber", "SecurityStamp", "CreationStep", "CreatedAt", "CreatedBy",
  "LastUpdatedAt", "LastUpdatedBy", "Address", "UndeliveredCashAmount", "PermissionsVersion"
)
SELECT
  'مندوب تحقق', 'أم', 'mandob_iso',
  (SELECT "PasswordHash" FROM "Users" WHERE "Id" = 1),
  'ISO-MANDOB', DATE '1990-01-01',
  '07000000002', gen_random_uuid(), 2, NOW(), 1, NOW(), 1, 'عنوان', 0, 0
WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'mandob_iso');

INSERT INTO "UserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "Users" u
CROSS JOIN "Roles" r
WHERE u."Username" = 'mandob_iso' AND r."Name" = 'Mandob'
  AND NOT EXISTS (
    SELECT 1 FROM "UserRoles" ur WHERE ur."UserId" = u."Id" AND ur."RoleId" = r."Id"
  );

INSERT INTO "OrderLists" (
  "Name", "MandobId", "MotabaId", "BranchId",
  "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy"
)
SELECT
  'قائمة تحقق',
  (SELECT "Id" FROM "Users" WHERE "Username" = 'mandob_iso'),
  (SELECT "Id" FROM "Users" WHERE "Username" = 'follower_iso'),
  (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع تحقق 20-21'),
  NOW(), 1, NOW(), 1
WHERE NOT EXISTS (SELECT 1 FROM "OrderLists" WHERE "Name" = 'قائمة تحقق');

-- Customer required for Orders
INSERT INTO "Customers" (
  "FullName", "MotherName", "NationalCode", "BirthDate", "PhoneNumber", "WhatsAppPhoneNumber",
  "Business_Address", "Business_Name", "Business_NearestKnownLocation",
  "BranchId", "IsLegal",
  "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy"
)
SELECT
  'زبون بيع تحقق', 'أم', 'ISO-CUST-1', DATE '1990-01-01', '07709998887', '07709998887',
  'عنوان زبون', 'نشاط', 'معلم',
  b."Id", false,
  NOW(), 1, NOW(), 1
FROM "Branches" b
WHERE b."Name" = 'فرع تحقق 20-21'
  AND NOT EXISTS (SELECT 1 FROM "Customers" WHERE "PhoneNumber" = '07709998887');

INSERT INTO "SalesRequests" (
  "CustomerName", "CustomerPhone", "Address", "RequestedProduct", "SubmissionNote",
  "SourceType", "BranchId", "Status", "IsDeleted",
  "ConcurrencyStamp", "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy",
  "SubmissionKey", "SubmissionFingerprint", "CreatedByUserId"
)
SELECT
  'زبون قانونية', '07701234567', 'العنوان الكامل للتحقق', 'منتج تحقق', NULL,
  0,
  (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع تحقق 20-21'),
  0, false,
  1, NOW(), 1, NOW(), 1,
  gen_random_uuid(), 'iso-fp', 1
WHERE NOT EXISTS (
  SELECT 1 FROM "SalesRequests" WHERE "CustomerPhone" = '07701234567' AND "CustomerName" = 'زبون قانونية'
);

-- Approved order for sales summary (SaleDate = 2026-11-09)
INSERT INTO "Orders" (
  "BuyAmount", "SellAmount", "PrepaymentAmount", "DailyInstallmentAmount",
  "CreationAddress", "SaleDate", "Step", "SellerId", "CustomerId", "OrderListId",
  "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy",
  "BranchId", "ApprovalStatus", "ExecutionStatus", "WorkflowStatus"
)
SELECT
  100000, 150000, 0, 1000,
  'عنوان', DATE '2026-11-09', 0, 1,
  (SELECT "Id" FROM "Customers" WHERE "PhoneNumber" = '07709998887'),
  (SELECT "Id" FROM "OrderLists" WHERE "Name" = 'قائمة تحقق'),
  NOW(), 1, NOW(), 1,
  (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع تحقق 20-21'),
  1, 0, 0
WHERE NOT EXISTS (
  SELECT 1 FROM "Orders" o
  JOIN "Customers" c ON c."Id" = o."CustomerId"
  WHERE c."PhoneNumber" = '07709998887' AND o."SellAmount" = 150000 AND o."SaleDate" = DATE '2026-11-09'
);

SELECT 'seed_done' AS status,
       (SELECT "Id" FROM "Branches" WHERE "Name" = 'فرع تحقق 20-21') AS branch_id,
       (SELECT "Id" FROM "Users" WHERE "Username" = 'follower_iso') AS follower_id,
       (SELECT "Id" FROM "SalesRequests" WHERE "CustomerPhone" = '07701234567' LIMIT 1) AS request_id,
       (SELECT COUNT(*) FROM "Orders" WHERE "ApprovalStatus" = 1) AS approved_orders;
