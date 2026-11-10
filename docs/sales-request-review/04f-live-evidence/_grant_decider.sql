SELECT current_database();
UPDATE "Users" SET "CreationStep"=3 WHERE "Username"='ex_decider_04f';
INSERT INTO "UserPermissions" ("UserId", "PermissionId")
SELECT u."Id", p."Id"
FROM "Users" u
CROSS JOIN "Permissions" p
WHERE u."Username"='ex_decider_04f'
  AND p."Name" IN (
    'SalesRequest.ExceptionDecide',
    'SalesRequest.TabExceptions',
    'SalesRequest.ReadUnassigned',
    'SalesRequest.TimelineRead'
  )
ON CONFLICT DO NOTHING;
SELECT u."Username", p."Name"
FROM "UserPermissions" up
JOIN "Users" u ON u."Id"=up."UserId"
JOIN "Permissions" p ON p."Id"=up."PermissionId"
WHERE u."Username"='ex_decider_04f';
