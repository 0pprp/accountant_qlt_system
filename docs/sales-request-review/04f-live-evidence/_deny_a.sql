SELECT current_database();
INSERT INTO "UserPermissionDenials" ("UserId", "PermissionId")
SELECT 3, p."Id" FROM "Permissions" p WHERE p."Name"='SalesRequest.AppRead'
ON CONFLICT DO NOTHING;
