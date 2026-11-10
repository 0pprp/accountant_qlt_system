SELECT current_database();
DO $$
DECLARE
  cid int; oid int; bid int; uid int;
BEGIN
  SELECT "Id" INTO bid FROM "Branches" ORDER BY "Id" LIMIT 1;
  SELECT "Id" INTO uid FROM "Users" ORDER BY "Id" LIMIT 1;
  INSERT INTO "Customers" (
    "FullName","MotherName","NationalCode","BirthDate",
    "PhoneNumber","WhatsAppPhoneNumber",
    "Business_Address","Business_Name","Business_NearestKnownLocation",
    "CreatedBy","LastUpdatedBy","BranchId"
  ) VALUES (
    '04F FK Probe','N/A','N/A','1990-01-01',
    '07000000001','07000000001','a','b','c',uid,uid,bid
  ) RETURNING "Id" INTO cid;
  INSERT INTO "Orders" (
    "BuyAmount","SellAmount","PrepaymentAmount","DailyInstallmentAmount",
    "Step","CustomerId","CreatedBy","LastUpdatedBy","BranchId","ExecutionStatus","ApprovalStatus"
  ) VALUES (1,1,0,0,0,cid,uid,uid,bid,0,0) RETURNING "Id" INTO oid;
  UPDATE "SalesRequests" SET "LinkedOrderId" = oid WHERE "Id" = 1;
  BEGIN
    DELETE FROM "Orders" WHERE "Id" = oid;
    RAISE EXCEPTION 'FK_RESTRICT_FAILED_TO_BLOCK';
  EXCEPTION WHEN SQLSTATE '23503' THEN
    RAISE NOTICE 'FK_LINKEDORDER_RESTRICT_OK';
  END;
  UPDATE "SalesRequests" SET "LinkedOrderId" = NULL WHERE "Id" = 1;
  DELETE FROM "Orders" WHERE "Id" = oid;
  DELETE FROM "Customers" WHERE "Id" = cid;
END $$;
