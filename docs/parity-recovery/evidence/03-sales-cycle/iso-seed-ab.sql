BEGIN;

INSERT INTO "RolePermissions" ("RoleId", "PermissionId")
SELECT 7, p."Id"
FROM "Permissions" p
WHERE p."Name" = 'Complaint.Submit'
  AND NOT EXISTS (
    SELECT 1 FROM "RolePermissions" rp
    WHERE rp."RoleId" = 7 AND rp."PermissionId" = p."Id"
  );

UPDATE "Users" u
SET "CreationStep" = 3,
    "PasswordHash" = (SELECT "PasswordHash" FROM "Users" WHERE "Username" = 'bm_iso'),
    "FullName" = 'موظف مبيعات A'
WHERE u."Username" = 'mandob_iso';

INSERT INTO "Users" (
  "FullName", "MotherName", "NationalCode", "BirthDate", "Username", "PasswordHash",
  "Address", "PhoneNumber", "UndeliveredCashAmount", "PermissionsVersion", "CreationStep",
  "CreatedAt", "CreatedBy", "LastUpdatedAt", "LastUpdatedBy", "SecurityStamp"
)
SELECT
  'موظف مبيعات B', "MotherName", 'NB00000002', "BirthDate", 'mandob_b', "PasswordHash",
  "Address", '07809990002', 0, 0, 3,
  NOW(), 1, NOW(), 1, gen_random_uuid()
FROM "Users"
WHERE "Username" = 'mandob_iso'
  AND NOT EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'mandob_b');

INSERT INTO "UserRoles" ("UserId", "RoleId")
SELECT u."Id", 7
FROM "Users" u
WHERE u."Username" = 'mandob_b'
  AND NOT EXISTS (SELECT 1 FROM "UserRoles" ur WHERE ur."UserId" = u."Id" AND ur."RoleId" = 7);

INSERT INTO "UserBranches" ("UserId", "BranchId")
SELECT u."Id", 1
FROM "Users" u
WHERE u."Username" IN ('mandob_iso', 'mandob_b')
  AND NOT EXISTS (
    SELECT 1 FROM "UserBranches" ub WHERE ub."UserId" = u."Id" AND ub."BranchId" = 1
  );

INSERT INTO "SalesEmployeeAssignments" (
  "UserId", "BranchId", "StartsAtUtc", "EndsAtUtc", "CreatedAtUtc", "CreatedByUserId", "Note"
)
SELECT u."Id", 1, NOW() - INTERVAL '1 day', NULL, NOW(), 1, 'iso A/B seed'
FROM "Users" u
WHERE u."Username" IN ('mandob_iso', 'mandob_b')
  AND NOT EXISTS (
    SELECT 1 FROM "SalesEmployeeAssignments" a
    WHERE a."UserId" = u."Id" AND a."BranchId" = 1 AND a."EndsAtUtc" IS NULL
  );

COMMIT;

SELECT u."Id", u."Username", u."CreationStep", u."FullName"
FROM "Users" u
WHERE u."Username" IN ('mandob_iso', 'mandob_b')
ORDER BY 1;

SELECT a."Id", a."UserId", u."Username", a."BranchId", a."EndsAtUtc"
FROM "SalesEmployeeAssignments" a
JOIN "Users" u ON u."Id" = a."UserId"
WHERE u."Username" IN ('mandob_iso', 'mandob_b');
