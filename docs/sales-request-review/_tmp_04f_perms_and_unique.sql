SELECT COUNT(*) AS orders FROM "Orders";

SELECT "Name" FROM "Permissions"
WHERE "Name" LIKE 'SalesRequest.%'
ORDER BY 1;

SELECT p."Name"
FROM "RolePermissions" rp
JOIN "Roles" r ON r."Id" = rp."RoleId"
JOIN "Permissions" p ON p."Id" = rp."PermissionId"
WHERE r."Name" = 'Admin' AND p."Name" LIKE 'SalesRequest.%'
ORDER BY 1;

SELECT p."Name"
FROM "RolePermissions" rp
JOIN "Roles" r ON r."Id" = rp."RoleId"
JOIN "Permissions" p ON p."Id" = rp."PermissionId"
WHERE r."Name" = 'Mandob' AND p."Name" LIKE 'SalesRequest.%'
ORDER BY 1;

SELECT COUNT(*) AS chief_sr
FROM "RolePermissions" rp
JOIN "Roles" r ON r."Id" = rp."RoleId"
JOIN "Permissions" p ON p."Id" = rp."PermissionId"
WHERE r."Name" = 'ChiefAccountant' AND p."Name" LIKE 'SalesRequest.%';

-- Unique LinkedOrderId: attach same order to two requests must fail if order exists
DO $$
DECLARE
  oid int;
  bid int;
BEGIN
  SELECT "Id" INTO oid FROM "Orders" ORDER BY "Id" LIMIT 1;
  SELECT "Id" INTO bid FROM "Branches" ORDER BY "Id" LIMIT 1;
  IF oid IS NULL THEN
    RAISE NOTICE 'NO_ORDER_SKIP_UNIQUE_TEST';
    RETURN;
  END IF;
  UPDATE "SalesRequests" SET "LinkedOrderId" = oid WHERE "Id" = 1;
  BEGIN
    INSERT INTO "SalesRequests" (
      "CustomerName","SourceType","Status","BranchId","IsDeleted","ConcurrencyStamp","CreatedBy","LinkedOrderId"
    ) VALUES ('dup-link',0,1,bid,false,0,0,oid);
    RAISE EXCEPTION 'UNIQUE_LINKEDORDER_FAILED_TO_BLOCK';
  EXCEPTION WHEN unique_violation THEN
    RAISE NOTICE 'UNIQUE_LINKEDORDER_OK';
  END;
END $$;
