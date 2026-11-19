-- Sandbox / isolated target only. Not applied to production until cutover gate passes.
-- Adds ImportKey and customer-level payment support for Najaf historical import.

ALTER TABLE "Customers" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Customers_ImportKey"
  ON "Customers" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "Orders" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Orders_ImportKey"
  ON "Orders" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "InstallmentPayments" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
ALTER TABLE "InstallmentPayments" ADD COLUMN IF NOT EXISTS "CustomerId" integer NULL;
ALTER TABLE "InstallmentPayments" ALTER COLUMN "OrderId" DROP NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS "UX_InstallmentPayments_ImportKey"
  ON "InstallmentPayments" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

DO $$
BEGIN
  IF NOT EXISTS (
    SELECT 1 FROM pg_constraint WHERE conname = 'FK_InstallmentPayments_Customers_CustomerId'
  ) THEN
    ALTER TABLE "InstallmentPayments"
      ADD CONSTRAINT "FK_InstallmentPayments_Customers_CustomerId"
      FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT;
  END IF;
END $$;
