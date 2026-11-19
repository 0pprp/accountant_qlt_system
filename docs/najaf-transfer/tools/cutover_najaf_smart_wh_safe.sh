#!/bin/bash
# Smart remap: one branch safe (خزينة النجف / BoxID=1 numbers) + products into مخزن النجف.
set -euo pipefail

ROOT=/root/najaf-transfer-BluaWosX
TS=$(TZ=Asia/Baghdad date +%Y%m%d-%H%M%S)
WORKDIR="$ROOT/smart-wh-safe-$TS"
SQLDIR="$WORKDIR/sql"
STAGE=najaf-pg-bluawosx
PROD=qalaat_postgres
BRANCH_ID=2
RATE=1448

mkdir -p "$WORKDIR" "$SQLDIR"
export PGPASSWORD=$(docker exec "$PROD" printenv POSTGRES_PASSWORD)
prod_c(){ docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1 -Atc "$1"; }
prod_file(){ docker cp "$1" "$PROD:/tmp/smart.sql"; docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1 -f /tmp/smart.sql; }
stage_c(){ docker exec "$STAGE" psql -U postgres -d postgres -v ON_ERROR_STOP=1 -Atc "$1"; }

RATE=$(stage_c "SELECT (payload->>'Amount')::float8 FROM source_stage.rows WHERE source_table='dbo.DollarAmount' ORDER BY 1 LIMIT 1")
RATE=${RATE:-1448}
SAFE_ID=$(prod_c "SELECT \"Id\" FROM \"Safes\" WHERE \"BranchId\"=$BRANCH_ID ORDER BY \"Id\" LIMIT 1")
WH_ID=$(prod_c "SELECT \"Id\" FROM \"Warehouses\" WHERE \"BranchId\"=$BRANCH_ID ORDER BY \"Id\" LIMIT 1")
echo "RATE=$RATE SAFE_ID=$SAFE_ID WH_ID=$WH_ID BRANCH_ID=$BRANCH_ID"

echo "== export box1 + items =="
stage_c "COPY (
  SELECT payload->>'WithdrawalFromBoxID',
         coalesce(payload->>'Amount','0'),
         left(coalesce(payload->>'Purpose', payload->>'Notes',''),400),
         coalesce(nullif(payload->>'DateCreate',''), nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows
  WHERE source_table='dbo.WithdrawalFromBox' AND payload->>'BoxID'='1'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/wd_box1.csv"

stage_c "COPY (
  SELECT coalesce(payload->>'AddToBoxID', payload->>'CustomerPaymentID'),
         coalesce(payload->>'AmountDenar', '0'),
         left(coalesce(payload->>'Notes',''),400),
         coalesce(nullif(payload->>'DateCreate',''), now()::text),
         CASE WHEN payload->>'CustomerIDPayment' IS NULL THEN 'nocust' ELSE 'cust' END
  FROM source_stage.rows
  WHERE source_table='dbo.View_AddToBox' AND payload->>'BoxID'='1'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/in_box1.csv"

stage_c "COPY (
  SELECT payload->>'ItemID', left(coalesce(payload->>'ItemName',''),100),
         coalesce(payload->>'Quantity','0'),
         coalesce(payload->>'ItemCost','0'),
         coalesce(payload->>'ItemPrice','0'),
         coalesce(payload->>'AmountDay','0'),
         left(coalesce(payload->>'Notes',''),400),
         coalesce(nullif(payload->>'CreatedDate',''), now()::text),
         coalesce(payload->>'ItemState','true')
  FROM source_stage.rows WHERE source_table='dbo.Items'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/items.csv"

wc -l "$WORKDIR"/*.csv

echo "== reset wrong safe txs; set safe name/totals from Box1 =="
docker cp "$WORKDIR/wd_box1.csv" "$PROD:/tmp/wd_box1.csv"
docker cp "$WORKDIR/in_box1.csv" "$PROD:/tmp/in_box1.csv"
cat > "$SQLDIR/01_safe.sql" <<SQL
BEGIN;
DELETE FROM "Transactions"
WHERE "SafeId" = $SAFE_ID
  AND ("ImportKey" LIKE 'najaf|WithdrawalFromBox|%' OR "ImportKey" LIKE 'najaf|AddToBoxNoCust|%' OR "ImportKey" LIKE 'najaf|AddToBox|%');

UPDATE "Safes"
SET "Name" = 'خزينة النجف',
    "LastUpdatedAt" = now(),
    "LastUpdatedBy" = 1
WHERE "Id" = $SAFE_ID;

CREATE TEMP TABLE raw_wd(id text, amount text, purpose text, created text);
COPY raw_wd FROM '/tmp/wd_box1.csv' WITH (FORMAT csv, NULL '');
INSERT INTO "Transactions" (
  "Amount","Type","Status","Direction","StatusDescription","SafeId",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  round((coalesce(nullif(amount,''),'0')::numeric * $RATE), 0)::float8,
  2, 1, 1,
  left(purpose,400),
  $SAFE_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|WithdrawalFromBox|', id)
FROM raw_wd
WHERE nullif(id,'') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM "Transactions" t WHERE t."ImportKey"=concat('najaf|WithdrawalFromBox|', raw_wd.id));

CREATE TEMP TABLE raw_in(id text, amount_denar text, notes text, created text, kind text);
COPY raw_in FROM '/tmp/in_box1.csv' WITH (FORMAT csv, NULL '');
INSERT INTO "Transactions" (
  "Amount","Type","Status","Direction","StatusDescription","SafeId",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  round(coalesce(nullif(amount_denar,''),'0')::numeric, 0)::float8,
  0, 1, 0,
  left(concat('[', kind, '] ', coalesce(notes,'')),400),
  $SAFE_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|AddToBox|', id)
FROM raw_in
WHERE nullif(id,'') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM "Transactions" t WHERE t."ImportKey"=concat('najaf|AddToBox|', raw_in.id));

UPDATE "Safes" s SET
  "DebitAmount" = coalesce((SELECT sum("Amount") FROM "Transactions" t WHERE t."SafeId"=s."Id" AND t."Direction"=1),0),
  "CreditAmount" = coalesce((SELECT sum("Amount") FROM "Transactions" t WHERE t."SafeId"=s."Id" AND t."Direction"=0),0),
  "RemainingCashAmount" = coalesce((SELECT sum(CASE WHEN t."Direction"=0 THEN t."Amount" ELSE -t."Amount" END) FROM "Transactions" t WHERE t."SafeId"=s."Id"),0),
  "LastUpdatedAt" = now(),
  "LastUpdatedBy" = 1
WHERE s."Id" = $SAFE_ID;
COMMIT;
SQL
prod_file "$SQLDIR/01_safe.sql"

echo "== smart products into مخزن النجف =="
docker cp "$WORKDIR/items.csv" "$PROD:/tmp/items_smart.csv"
cat > "$SQLDIR/02_products.sql" <<SQL
BEGIN;
UPDATE "Warehouses"
SET "Name"='مخزن النجف', "ImportKey"=coalesce("ImportKey",'najaf|Stores|default')
WHERE "Id"=$WH_ID AND "BranchId"=$BRANCH_ID;

INSERT INTO "ProductCategories" ("Name","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey")
SELECT v.name, now(), 1, now(), 1, v.ikey
FROM (VALUES
  ('موبايلات','najaf|Cat|phones'),
  ('دراجات','najaf|Cat|bikes'),
  ('شاشات وتلفزيون','najaf|Cat|screens'),
  ('أجهزة منزلية','najaf|Cat|home'),
  ('أثاث ومساج','najaf|Cat|furniture'),
  ('أخرى','najaf|Cat|other')
) AS v(name, ikey)
WHERE NOT EXISTS (SELECT 1 FROM "ProductCategories" c WHERE c."ImportKey"=v.ikey);

CREATE TEMP TABLE raw_items(
  item_id text, name text, qty text, cost text, price text, day_amt text, notes text, created text, state text
);
COPY raw_items FROM '/tmp/items_smart.csv' WITH (FORMAT csv, NULL '');

INSERT INTO "Products" (
  "Name","RemainingCount","BuyAmount","SellAmount","DailyInstallmentAmount","Description",
  "CategoryId","WarehouseId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  CASE WHEN length(trim(name))=0 THEN left(concat('Item-', item_id),100) ELSE left(name,100) END,
  greatest(0, floor(coalesce(nullif(qty,''),'0')::float8)::int),
  round((coalesce(nullif(cost,''),'0')::numeric * $RATE), 0)::float8,
  round((coalesce(nullif(price,''),'0')::numeric * $RATE), 0)::float8,
  round((coalesce(nullif(day_amt,''),'0')::numeric * $RATE), 0)::float8,
  nullif(notes,''),
  CASE
    WHEN name ILIKE '%موبايل%' OR name ILIKE '%ايفون%' OR name ILIKE '%آيفون%'
      OR name ILIKE '%سامسونج%' OR name ILIKE '%ريدمي%' OR name ILIKE '%ريلمي%'
      OR name ILIKE '%انفنكس%' OR name ILIKE '%اننفنكس%' OR name ILIKE '%نوت%'
      OR name ILIKE '%كامون%' OR name ILIKE '%هوت%' OR name ILIKE '%بوكو%'
      THEN (SELECT "Id" FROM "ProductCategories" WHERE "ImportKey"='najaf|Cat|phones')
    WHEN name ILIKE '%دراجة%' OR name ILIKE '%سيكل%'
      THEN (SELECT "Id" FROM "ProductCategories" WHERE "ImportKey"='najaf|Cat|bikes')
    WHEN name ILIKE '%شاشة%' OR name ILIKE '%تلفزيون%' OR name ILIKE '%TV%'
      THEN (SELECT "Id" FROM "ProductCategories" WHERE "ImportKey"='najaf|Cat|screens')
    WHEN name ILIKE '%كرسي%' OR name ILIKE '%مساج%' OR name ILIKE '%سرير%'
      THEN (SELECT "Id" FROM "ProductCategories" WHERE "ImportKey"='najaf|Cat|furniture')
    WHEN name ILIKE '%مكنسة%' OR name ILIKE '%طباخ%' OR name ILIKE '%سبلت%'
      OR name ILIKE '%ثلاجة%' OR name ILIKE '%غسالة%'
      THEN (SELECT "Id" FROM "ProductCategories" WHERE "ImportKey"='najaf|Cat|home')
    ELSE (SELECT "Id" FROM "ProductCategories" WHERE "ImportKey"='najaf|Cat|other')
  END,
  $WH_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|Items|', item_id)
FROM raw_items
WHERE NOT EXISTS (SELECT 1 FROM "Products" p WHERE p."ImportKey"=concat('najaf|Items|', raw_items.item_id));

INSERT INTO najaf_import.map(import_key, entity_type, entity_id, source_table)
SELECT p."ImportKey", 'Product', p."Id", 'Items'
FROM "Products" p WHERE p."ImportKey" LIKE 'najaf|Items|%'
ON CONFLICT DO NOTHING;
COMMIT;
SQL
prod_file "$SQLDIR/02_products.sql"

echo "== match =="
prod_c "SELECT concat('safe_name|', \"Name\", '|remain|', round(\"RemainingCashAmount\"::numeric,0), '|credit|', round(\"CreditAmount\"::numeric,0), '|debit|', round(\"DebitAmount\"::numeric,0)) FROM \"Safes\" WHERE \"Id\"=$SAFE_ID"
prod_c "SELECT concat('safe_tx_in|', count(*)) FROM \"Transactions\" WHERE \"SafeId\"=$SAFE_ID AND \"Direction\"=0 AND \"ImportKey\" LIKE 'najaf|AddToBox|%'"
prod_c "SELECT concat('safe_tx_out|', count(*)) FROM \"Transactions\" WHERE \"SafeId\"=$SAFE_ID AND \"Direction\"=1 AND \"ImportKey\" LIKE 'najaf|WithdrawalFromBox|%'"
prod_c "SELECT concat('products|', count(*), '|qty|', coalesce(sum(\"RemainingCount\"),0)) FROM \"Products\" WHERE \"ImportKey\" LIKE 'najaf|Items|%'"
prod_c "SELECT concat('wh|', \"Name\") FROM \"Warehouses\" WHERE \"Id\"=$WH_ID"
stage_c "SELECT concat('src_box1_remain|', round((SELECT coalesce(sum((payload->>'AmountDenar')::numeric),0) FROM source_stage.rows WHERE source_table='dbo.View_AddToBox' AND payload->>'BoxID'='1') - (SELECT coalesce(sum((payload->>'Amount')::numeric),0) FROM source_stage.rows WHERE source_table='dbo.WithdrawalFromBox' AND payload->>'BoxID'='1') * $RATE, 0))"
stage_c "SELECT concat('src_items|', count(*), '|qty|', coalesce(sum((payload->>'Quantity')::float8),0)::bigint) FROM source_stage.rows WHERE source_table='dbo.Items'"
echo "SMART_WH_SAFE_DONE workdir=$WORKDIR"
