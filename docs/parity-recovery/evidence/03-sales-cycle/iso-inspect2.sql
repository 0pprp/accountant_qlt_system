SELECT u."Id", u."Username", u."CreationStep", r."Name" AS role
FROM "Users" u
LEFT JOIN "UserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "Roles" r ON r."Id" = ur."RoleId"
ORDER BY u."Id";
SELECT * FROM "UserBranches";
SELECT "RoleId", p."Name"
FROM "RolePermissions" rp
JOIN "Permissions" p ON p."Id" = rp."PermissionId"
WHERE rp."RoleId" = 7 AND (p."Name" LIKE 'SalesRequest.App%' OR p."Name" IN ('Complaint.Submit','AppNote.Create'))
ORDER BY 2;
