SELECT current_database();
UPDATE "Orders" SET "SellerId"=4 WHERE "Id"=4;
SELECT "Id","SellerId","ApprovalStatus" FROM "Orders" WHERE "Id"=4;
