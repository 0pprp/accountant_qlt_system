SELECT u."Id", u."Username", r."Name" AS role
FROM "Users" u
LEFT JOIN "UserRoles" ur ON ur."UserId"=u."Id"
LEFT JOIN "Roles" r ON r."Id"=ur."RoleId"
WHERE u."Username" IN ('emp_a_04f','emp_b_04f');
SELECT "Id","Name" FROM "Roles" LIMIT 20;
SELECT column_name, data_type FROM information_schema.columns WHERE table_name='OrderLists' ORDER BY ordinal_position;
