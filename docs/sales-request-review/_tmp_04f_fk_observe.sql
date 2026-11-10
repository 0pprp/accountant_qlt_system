-- Observe FK restrict without swallowing (constraint proof = blocked delete)
SELECT current_database();
UPDATE "SalesRequests" SET "LinkedOrderId" = NULL WHERE "LinkedOrderId" IS NOT NULL;
DELETE FROM "Orders" o WHERE NOT EXISTS (
  SELECT 1 FROM "SalesRequests" s WHERE s."LinkedOrderId" = o."Id"
) AND o."Id" >= 1;
DELETE FROM "Customers" WHERE "FullName" LIKE '04F %';

DO $$
DECLARE
  cid int; oid int; bid int; uid int; sqlstate_txt text; sqlerrm_txt text;
BEGIN
  SELECT "Id" INTO bid FROM "Branches" ORDER BY "Id" LIMIT 1;
  SELECT "Id" INTO uid FROM "Users" ORDER BY "Id" LIMIT 1;
  INSERT INTO "Customers" (
    "FullName","MotherName","NationalCode","BirthDate",
    "PhoneNumber","WhatsAppPhoneNumber",
    "Business_Address","Business_Name","Business_NearestKnownLocation",
    "CreatedBy","LastUpdatedBy","BranchId"
  ) VALUES (
    '04F FK Observe','N/A','N/A','1990-01-01',
    '07000000002','07000000002','a','b','c',uid,uid,bid
  ) RETURNING "Id" INTO cid;
  INSERT INTO "Orders" (
    "BuyAmount","SellAmount","PrepaymentAmount","DailyInstallmentAmount",
    "Step","CustomerId","CreatedBy","LastUpdatedBy","BranchId","ExecutionStatus","ApprovalStatus"
  ) VALUES (1,1,0,0,0,cid,uid,uid,bid,0,0) RETURNING "Id" INTO oid;
  UPDATE "SalesRequests" SET "LinkedOrderId" = oid WHERE "Id" = 1;
  BEGIN
    DELETE FROM "Orders" WHERE "Id" = oid;
  EXCEPTION WHEN OTHERS THEN
    GET STACKED DIAGNOSTICS sqlstate_txt = RETURNED_SQLSTATE, sqlerrm_txt = MESSAGE_TEXT;
    RAISE NOTICE 'FK_OBSERVED sqlstate=% msg=%', sqlstate_txt, sqlerrm_txt;
  END;
  UPDATE "SalesRequests" SET "LinkedOrderId" = NULL WHERE "Id" = 1;
  DELETE FROM "Orders" WHERE "Id" = oid;
  DELETE FROM "Customers" WHERE "Id" = cid;
END $$;
