SELECT current_database();
SELECT u."Id", u."Username", u."CreationStep",
       EXISTS (SELECT 1 FROM "UserBranches" ub WHERE ub."UserId"=u."Id" AND ub."BranchId"=1) AS has_branch,
       (SELECT string_agg(r."Name", ',') FROM "UserRoles" ur JOIN "Roles" r ON r."Id"=ur."RoleId" WHERE ur."UserId"=u."Id") AS roles
FROM "Users" u
WHERE u."Username" IN ('emp_a_04f','emp_b_04f','admin');
