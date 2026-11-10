SELECT u."Id", u."UserName", r."Name"
FROM "Users" u
JOIN "UserRoles" ur ON ur."UserId"=u."Id"
JOIN "Roles" r ON r."Id"=ur."RoleId"
WHERE r."Name" ILIKE '%Motaba%' OR r."Name" ILIKE '%متابع%'
LIMIT 15;

SELECT ol."Id", ol."Name", ol."MotabaId", ol."MandobId", ol."FollowerCollectionEnabled"
FROM "OrderLists" ol
WHERE ol."MotabaId" IS NOT NULL
LIMIT 15;
