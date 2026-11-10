-- Clean wrong attachments on order 7
DELETE FROM "Attachments" WHERE "OrderId"=7;

-- Ensure Motaba users exist for lists (reuse emp if needed — find Motaba)
SELECT u."Id", u."Username", r."Name"
FROM "Users" u
JOIN "UserRoles" ur ON ur."UserId"=u."Id"
JOIN "Roles" r ON r."Id"=ur."RoleId"
WHERE r."Name"='Motaba';

-- Create OrderLists for Mandob A and B if missing
INSERT INTO "OrderLists" ("Name","MandobId","MotabaId","BranchId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy")
SELECT 'قائمة موظف A', 3, COALESCE((SELECT u."Id" FROM "Users" u JOIN "UserRoles" ur ON ur."UserId"=u."Id" JOIN "Roles" r ON r."Id"=ur."RoleId" WHERE r."Name"='Motaba' LIMIT 1), 3), 1, NOW(), 1, NOW(), 1
WHERE NOT EXISTS (SELECT 1 FROM "OrderLists" WHERE "MandobId"=3);

INSERT INTO "OrderLists" ("Name","MandobId","MotabaId","BranchId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy")
SELECT 'قائمة موظف B', 4, COALESCE((SELECT u."Id" FROM "Users" u JOIN "UserRoles" ur ON ur."UserId"=u."Id" JOIN "Roles" r ON r."Id"=ur."RoleId" WHERE r."Name"='Motaba' LIMIT 1), 4), 1, NOW(), 1, NOW(), 1
WHERE NOT EXISTS (SELECT 1 FROM "OrderLists" WHERE "MandobId"=4);

SELECT "Id","Name","MandobId","MotabaId","BranchId" FROM "OrderLists";
