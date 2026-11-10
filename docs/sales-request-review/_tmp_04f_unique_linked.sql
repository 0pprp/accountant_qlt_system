SELECT current_database();

DO $$
DECLARE
  cid int;
  oid int;
  bid int;
  uid int;
BEGIN
  SELECT "Id" INTO bid FROM "Branches" ORDER BY "Id" LIMIT 1;
  SELECT "Id" INTO uid FROM "Users" ORDER BY "Id" LIMIT 1;

  INSERT INTO "Customers" (
    "FullName","MotherName","NationalCode","BirthDate",
    "PhoneNumber","WhatsAppPhoneNumber",
    "Business_Address","Business_Name","Business_NearestKnownLocation",
    "CreatedBy","LastUpdatedBy","BranchId"
  ) VALUES (
    '04F Unique Probe','N/A','N/A','1990-01-01',
    '07000000000','07000000000',
    'addr','biz','near',
    uid,uid,bid
  ) RETURNING "Id" INTO cid;

  INSERT INTO "Orders" (
    "BuyAmount","SellAmount","PrepaymentAmount","DailyInstallmentAmount",
    "Step","CustomerId","CreatedBy","LastUpdatedBy","BranchId","ExecutionStatus","ApprovalStatus"
  ) VALUES (100,150,10,5,0,cid,uid,uid,bid,0,0)
  RETURNING "Id" INTO oid;

  UPDATE "SalesRequests" SET "LinkedOrderId" = oid WHERE "Id" = 1;

  BEGIN
    INSERT INTO "SalesRequests" (
      "CustomerName","SourceType","Status","BranchId","IsDeleted","ConcurrencyStamp","CreatedBy","LinkedOrderId"
    ) VALUES ('dup-link',0,1,bid,false,0,0,oid);
    RAISE EXCEPTION 'UNIQUE_LINKEDORDER_FAILED_TO_BLOCK';
  EXCEPTION WHEN unique_violation THEN
    RAISE NOTICE 'UNIQUE_LINKEDORDER_OK order=%', oid;
  END;

  -- FK restrict: delete order while linked must fail
  BEGIN
    DELETE FROM "Orders" WHERE "Id" = oid;
    RAISE EXCEPTION 'FK_RESTRICT_FAILED_TO_BLOCK';
  EXCEPTION WHEN foreign_key_violation THEN
    RAISE NOTICE 'FK_LINKEDORDER_RESTRICT_OK';
  END;

  UPDATE "SalesRequests" SET "LinkedOrderId" = NULL WHERE "Id" = 1;
  DELETE FROM "Orders" WHERE "Id" = oid;
  DELETE FROM "Customers" WHERE "Id" = cid;
END $$;
