SELECT "Id","Name" FROM "Roles" ORDER BY "Id";
SELECT "Id","Username" FROM "Users" ORDER BY "Id";
SELECT u."Id", u."Username", r."Name" AS role
FROM "Users" u
LEFT JOIN "UserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "Roles" r ON r."Id" = ur."RoleId"
ORDER BY u."Id";
