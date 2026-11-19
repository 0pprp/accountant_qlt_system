#!/bin/bash
# Production cutover: literal Najaf financial copy from source_stage -> qalaat-db.
set -euo pipefail

ROOT=/root/najaf-transfer-BluaWosX
TS=$(TZ=Asia/Baghdad date +%Y%m%d-%H%M%S)
BACKUP_DIR="$ROOT/backups/cutover-$TS"
STAGE=najaf-pg-bluawosx
PROD=qalaat_postgres
BRANCH_NAME='النجف'
WORKDIR="$ROOT/cutover-work-$TS"
SQLDIR="$WORKDIR/sql"

mkdir -p "$BACKUP_DIR" "$WORKDIR" "$SQLDIR"
echo "BACKUP_DIR=$BACKUP_DIR"

export PGPASSWORD=$(docker exec "$PROD" printenv POSTGRES_PASSWORD)
prod_f() { docker exec -i -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1; }
prod_c() { docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1 -Atc "$1"; }
prod_file() { docker cp "$1" "$PROD:/tmp/cutover.sql"; docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1 -f /tmp/cutover.sql; }
stage_c() { docker exec "$STAGE" psql -U postgres -d postgres -v ON_ERROR_STOP=1 -Atc "$1"; }

echo "== 1) backup =="
if [[ ! -f "$ROOT/backups/cutover-20261117-024546/qalaat-pre-najaf.dump" ]]; then
  docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" \
    pg_dump -U qalaatal -d qalaat-db -Fc -f /tmp/qalaat-pre-najaf.dump
  docker cp "$PROD:/tmp/qalaat-pre-najaf.dump" "$BACKUP_DIR/qalaat-pre-najaf.dump"
else
  cp -a "$ROOT/backups/cutover-20261117-024546/qalaat-pre-najaf.dump" "$BACKUP_DIR/" || true
  docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" \
    pg_dump -U qalaatal -d qalaat-db -Fc -f /tmp/qalaat-pre-najaf.dump
  docker cp "$PROD:/tmp/qalaat-pre-najaf.dump" "$BACKUP_DIR/qalaat-pre-najaf.dump"
fi
sha256sum "$BACKUP_DIR/qalaat-pre-najaf.dump" | tee "$BACKUP_DIR/qalaat-pre-najaf.dump.sha256"
ahmed_before=$(prod_c 'SELECT count(*) FROM "Users" WHERE lower("Username")='"'"'ahmed'"'"'')
[[ "$ahmed_before" == "1" ]] || { echo "ahmed missing before cutover"; exit 1; }

echo "== 2) schema =="
cat > "$SQLDIR/01_schema.sql" <<'SQL'
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

CREATE SCHEMA IF NOT EXISTS najaf_import;
CREATE TABLE IF NOT EXISTS najaf_import.map (
  import_key text PRIMARY KEY,
  entity_type text NOT NULL,
  entity_id bigint NOT NULL,
  source_table text NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now()
);
CREATE TABLE IF NOT EXISTS najaf_import.cutover_state (
  key text PRIMARY KEY,
  value text NOT NULL,
  updated_at timestamptz NOT NULL DEFAULT now()
);
SQL
prod_file "$SQLDIR/01_schema.sql"

BRANCH_ID=$(prod_c "SELECT \"Id\" FROM \"Branches\" WHERE \"Name\" = '$BRANCH_NAME' LIMIT 1")
[[ -n "$BRANCH_ID" ]] || { echo "branch missing"; exit 1; }
echo "BRANCH_ID=$BRANCH_ID"

echo "== 3) export stage extracts =="
stage_c "COPY (
  SELECT
    payload->>'CustomerID',
    left(coalesce(payload->>'CustomerName',''),50),
    left(coalesce(payload->>'PhoneNumber',''),30),
    left(coalesce(payload->>'ShopName', payload->>'CustomerName',''),100),
    left(coalesce(payload->>'Address',''),300),
    left(coalesce(payload->>'Neighborhood',''),200),
    coalesce(payload->>'IsLegal','false'),
    coalesce(payload->>'IsFakeSale','false'),
    nullif(payload->>'Latitude',''),
    nullif(payload->>'Longitude',''),
    coalesce(nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.Customers'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/customers.csv"

stage_c "COPY (
  SELECT
    payload->>'CustomerSaleID',
    payload->>'CustomerID',
    payload->>'AmountTotalSalesDenar',
    coalesce(nullif(payload->>'DateCreate',''), nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.View_CustomersSales'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/sales.csv"

stage_c "COPY (
  SELECT
    coalesce(payload->>'CustomerPaymentID', payload->>'AddToBoxID'),
    payload->>'CustomerIDPayment',
    payload->>'AmountDenar',
    coalesce(nullif(payload->>'DateCreate',''), now()::text)
  FROM source_stage.rows
  WHERE source_table='dbo.View_AddToBox'
    AND payload->>'CustomerIDPayment' IS NOT NULL
    AND payload->>'AmountDenar' IS NOT NULL
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/payments.csv"

wc -l "$WORKDIR"/*.csv | tee "$BACKUP_DIR/export_counts.txt"

echo "== 4) load customers =="
docker cp "$WORKDIR/customers.csv" "$PROD:/tmp/najaf_customers.csv"
cat > "$SQLDIR/02_customers.sql" <<SQL
CREATE TEMP TABLE najaf_cust_raw (
  src_id text, full_name text, phone text, shop text, address text, near text,
  is_legal text, is_fake text, lat text, lng text, created text
);
COPY najaf_cust_raw FROM '/tmp/najaf_customers.csv' WITH (FORMAT csv, NULL '');

INSERT INTO "Customers" (
  "FullName","MotherName","NationalCode","BirthDate",
  "PhoneNumber","WhatsAppPhoneNumber",
  "Business_Name","Business_Address","Business_NearestKnownLocation",
  "Business_GpsLatitude","Business_GpsLongitude",
  "BranchId","IsLegal","IsFakeSale",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  CASE WHEN length(full_name)=0 THEN left(concat('Najaf-', src_id),50) ELSE full_name END,
  '',
  '',
  DATE '1970-01-01',
  coalesce(phone,''),
  coalesce(phone,''),
  CASE WHEN length(coalesce(shop,''))=0 THEN left(coalesce(full_name,'Najaf'),100) ELSE shop END,
  coalesce(address,''),
  coalesce(near,''),
  NULLIF(lat,'')::float8,
  NULLIF(lng,'')::float8,
  $BRANCH_ID,
  lower(is_legal) IN ('true','t','1'),
  lower(is_fake) IN ('true','t','1'),
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|Customers|', src_id)
FROM najaf_cust_raw raw
WHERE NOT EXISTS (
  SELECT 1 FROM "Customers" c WHERE c."ImportKey" = concat('najaf|Customers|', raw.src_id)
);

INSERT INTO najaf_import.map (import_key, entity_type, entity_id, source_table)
SELECT c."ImportKey", 'Customer', c."Id", 'Customers'
FROM "Customers" c
WHERE c."ImportKey" LIKE 'najaf|Customers|%'
ON CONFLICT (import_key) DO NOTHING;
SQL
prod_file "$SQLDIR/02_customers.sql"

echo "== 5) load orders =="
docker cp "$WORKDIR/sales.csv" "$PROD:/tmp/najaf_sales.csv"
cat > "$SQLDIR/03_orders.sql" <<SQL
CREATE TEMP TABLE najaf_sale_raw (
  sale_id text, customer_src text, amount_text text, created text
);
COPY najaf_sale_raw FROM '/tmp/najaf_sales.csv' WITH (FORMAT csv, NULL '');

INSERT INTO "Orders" (
  "BuyAmount","SellAmount","PrepaymentAmount","DailyInstallmentAmount",
  "Step","ExecutionStatus","ApprovalStatus","WorkflowStatus",
  "CustomerId","BranchId","SaleDate","SalesCompletedAtUtc",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  amount_text::float8,
  amount_text::float8,
  0::float8,
  0::float8,
  2::smallint,
  2::smallint,
  1::smallint,
  5::smallint,
  c."Id",
  $BRANCH_ID,
  (created::timestamptz)::date,
  created::timestamptz,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|CustomersSales|', sale_id)
FROM najaf_sale_raw raw
JOIN "Customers" c ON c."ImportKey" = concat('najaf|Customers|', raw.customer_src)
WHERE NOT EXISTS (
  SELECT 1 FROM "Orders" o WHERE o."ImportKey" = concat('najaf|CustomersSales|', raw.sale_id)
);

INSERT INTO najaf_import.map (import_key, entity_type, entity_id, source_table)
SELECT o."ImportKey", 'Order', o."Id", 'CustomersSales'
FROM "Orders" o
WHERE o."ImportKey" LIKE 'najaf|CustomersSales|%'
ON CONFLICT (import_key) DO NOTHING;
SQL
prod_file "$SQLDIR/03_orders.sql"

echo "== 6) load payments =="
docker cp "$WORKDIR/payments.csv" "$PROD:/tmp/najaf_payments.csv"
cat > "$SQLDIR/04_payments.sql" <<SQL
CREATE TEMP TABLE najaf_pay_raw (
  pay_id text, customer_src text, amount_text text, created text
);
COPY najaf_pay_raw FROM '/tmp/najaf_payments.csv' WITH (FORMAT csv, NULL '');

INSERT INTO "InstallmentPayments" (
  "Amount","Date","Description","OrderId","CustomerId","ClientPaymentId",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  amount_text::float8,
  (created::timestamptz)::date,
  'najaf-historical',
  NULL,
  c."Id",
  NULL,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|CustomersPayments|', pay_id)
FROM najaf_pay_raw raw
JOIN "Customers" c ON c."ImportKey" = concat('najaf|Customers|', raw.customer_src)
WHERE NOT EXISTS (
  SELECT 1 FROM "InstallmentPayments" p WHERE p."ImportKey" = concat('najaf|CustomersPayments|', raw.pay_id)
);

INSERT INTO najaf_import.map (import_key, entity_type, entity_id, source_table)
SELECT p."ImportKey", 'InstallmentPayment', p."Id", 'CustomersPayments'
FROM "InstallmentPayments" p
WHERE p."ImportKey" LIKE 'najaf|CustomersPayments|%'
ON CONFLICT (import_key) DO NOTHING;
SQL
prod_file "$SQLDIR/04_payments.sql"

echo "== 7) match =="
cat > "$SQLDIR/05_match.sql" <<'SQL'
SELECT 'tgt_customers' k, count(*)::text n FROM "Customers" WHERE "ImportKey" LIKE 'najaf|Customers|%';
SELECT 'tgt_orders' k, concat(count(*)::text, '|', coalesce(sum("SellAmount"),0)::text) FROM "Orders" WHERE "ImportKey" LIKE 'najaf|CustomersSales|%';
SELECT 'tgt_payments' k, concat(count(*)::text, '|', coalesce(sum("Amount"),0)::text) FROM "InstallmentPayments" WHERE "ImportKey" LIKE 'najaf|CustomersPayments|%';
SELECT 'ahmed' k, count(*)::text FROM "Users" WHERE lower("Username")='ahmed';
SQL
prod_file "$SQLDIR/05_match.sql" | tee "$BACKUP_DIR/post_cutover_match.txt"

stage_c "SELECT concat('src_customers|', count(*)) FROM source_stage.rows WHERE source_table='dbo.Customers'" | tee "$BACKUP_DIR/source_match.txt"
stage_c "SELECT concat('src_sales|', count(*), '|', sum((payload->>'AmountTotalSalesDenar')::float8)) FROM source_stage.rows WHERE source_table='dbo.View_CustomersSales'" | tee -a "$BACKUP_DIR/source_match.txt"
stage_c "SELECT concat('src_pay|', count(*), '|', sum((payload->>'AmountDenar')::float8)) FROM source_stage.rows WHERE source_table='dbo.View_AddToBox' AND payload->>'CustomerIDPayment' IS NOT NULL AND payload->>'AmountDenar' IS NOT NULL" | tee -a "$BACKUP_DIR/source_match.txt"

ahmed_after=$(prod_c 'SELECT count(*) FROM "Users" WHERE lower("Username")='"'"'ahmed'"'"'')
[[ "$ahmed_after" == "1" ]] || { echo "ahmed changed — FAIL"; exit 1; }

cat > "$SQLDIR/06_state.sql" <<SQL
INSERT INTO najaf_import.cutover_state(key,value) VALUES
 ('baghdad_ts','$TS'),('backup','$BACKUP_DIR'),('branch_id','$BRANCH_ID')
ON CONFLICT (key) DO UPDATE SET value=EXCLUDED.value, updated_at=now();
SQL
prod_file "$SQLDIR/06_state.sql"

echo "CUTOVER_DONE backup=$BACKUP_DIR"
