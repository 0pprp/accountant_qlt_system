-- Minimal Qalaat-shaped sandbox for Najaf import rehearsal (isolated PG).
CREATE SCHEMA IF NOT EXISTS public;

CREATE TABLE IF NOT EXISTS "Branches" (
  "Id" serial PRIMARY KEY,
  "Name" text NOT NULL,
  "Created" timestamptz NOT NULL DEFAULT now(),
  "CreatedBy" text
);

CREATE TABLE IF NOT EXISTS "AspNetUsers" (
  "Id" serial PRIMARY KEY,
  "UserName" varchar(256) NOT NULL
);

INSERT INTO "AspNetUsers" ("UserName")
SELECT 'ahmed' WHERE NOT EXISTS (SELECT 1 FROM "AspNetUsers" WHERE lower("UserName")='ahmed');

CREATE TABLE IF NOT EXISTS "Customers" (
  "Id" serial PRIMARY KEY,
  "FullName" varchar(100) NOT NULL,
  "MotherName" varchar(100) NOT NULL DEFAULT '',
  "NationalCode" varchar(50) NOT NULL DEFAULT '',
  "BirthDate" date NOT NULL DEFAULT '1970-01-01',
  "PhoneNumber" varchar(30) NOT NULL DEFAULT '',
  "WhatsAppPhoneNumber" varchar(30) NOT NULL DEFAULT '',
  "BranchId" int NOT NULL REFERENCES "Branches"("Id"),
  "IsLegal" boolean NOT NULL DEFAULT false,
  "IsFakeSale" boolean NOT NULL DEFAULT false,
  "ImportKey" varchar(120) NULL,
  "Created" timestamptz NOT NULL DEFAULT now(),
  "CreatedBy" text
);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Customers_ImportKey"
  ON "Customers" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

CREATE TABLE IF NOT EXISTS "Orders" (
  "Id" serial PRIMARY KEY,
  "BuyAmount" double precision NOT NULL DEFAULT 0,
  "SellAmount" double precision NOT NULL DEFAULT 0,
  "PrepaymentAmount" double precision NOT NULL DEFAULT 0,
  "DailyInstallmentAmount" double precision NOT NULL DEFAULT 0,
  "CustomerId" int NOT NULL REFERENCES "Customers"("Id"),
  "BranchId" int NOT NULL REFERENCES "Branches"("Id"),
  "ImportKey" varchar(120) NULL,
  "Created" timestamptz NOT NULL DEFAULT now(),
  "CreatedBy" text
);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Orders_ImportKey"
  ON "Orders" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

CREATE TABLE IF NOT EXISTS "InstallmentPayments" (
  "Id" serial PRIMARY KEY,
  "Amount" double precision NOT NULL,
  "Date" date NOT NULL,
  "Description" varchar(400),
  "ClientPaymentId" uuid NULL,
  "ImportKey" varchar(120) NULL,
  "OrderId" int NULL REFERENCES "Orders"("Id"),
  "CustomerId" int NULL REFERENCES "Customers"("Id"),
  "Created" timestamptz NOT NULL DEFAULT now(),
  "CreatedBy" text
);
CREATE UNIQUE INDEX IF NOT EXISTS "UX_InstallmentPayments_ImportKey"
  ON "InstallmentPayments" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

CREATE SCHEMA IF NOT EXISTS najaf_import;
CREATE TABLE IF NOT EXISTS najaf_import.map (
  import_key text PRIMARY KEY,
  entity_type text NOT NULL,
  entity_id bigint NOT NULL,
  source_table text NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS najaf_import.exceptions (
  id bigserial PRIMARY KEY,
  import_key text,
  step text NOT NULL,
  detail jsonb NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now()
);

INSERT INTO "Branches" ("Name", "CreatedBy")
SELECT 'النجف', 'najaf-import'
WHERE NOT EXISTS (SELECT 1 FROM "Branches" WHERE "Name" = 'النجف');
