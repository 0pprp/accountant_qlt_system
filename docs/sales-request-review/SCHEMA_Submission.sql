BEGIN;
DO $$ BEGIN
IF current_database() <> 'qalaat_04f_sales_verify' THEN
RAISE EXCEPTION 'This execution script is restricted to the isolated verification database';
END IF;
END $$;
ALTER TABLE "SalesRequests" ADD COLUMN IF NOT EXISTS "Address" varchar(500);
ALTER TABLE "SalesRequests" ADD COLUMN IF NOT EXISTS "RequestedProduct" varchar(200);
ALTER TABLE "SalesRequests" ADD COLUMN IF NOT EXISTS "SubmissionNote" varchar(1000);
ALTER TABLE "SalesRequests" ADD COLUMN IF NOT EXISTS "SubmissionKey" uuid;
ALTER TABLE "SalesRequests" ADD COLUMN IF NOT EXISTS "SubmissionFingerprint" varchar(64);
CREATE UNIQUE INDEX IF NOT EXISTS "IX_SalesRequests_CreatedByUserId_SubmissionKey"
ON "SalesRequests" ("CreatedByUserId", "SubmissionKey");
COMMIT;
