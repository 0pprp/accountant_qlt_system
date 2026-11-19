#!/bin/bash
# Phase 2: warehouses, products, purchases, order lists, safe movements — literal from source_stage.
set -euo pipefail

ROOT=/root/najaf-transfer-BluaWosX
TS=$(TZ=Asia/Baghdad date +%Y%m%d-%H%M%S)
BACKUP_DIR="$ROOT/backups/cutover-phase2-$TS"
WORKDIR="$ROOT/cutover-p2-$TS"
SQLDIR="$WORKDIR/sql"
STAGE=najaf-pg-bluawosx
PROD=qalaat_postgres
BRANCH_ID=2
RATE=1448
MOTABA_ID=1

mkdir -p "$BACKUP_DIR" "$WORKDIR" "$SQLDIR"
export PGPASSWORD=$(docker exec "$PROD" printenv POSTGRES_PASSWORD)
prod_c(){ docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1 -Atc "$1"; }
prod_file(){ docker cp "$1" "$PROD:/tmp/p2.sql"; docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" psql -U qalaatal -d qalaat-db -v ON_ERROR_STOP=1 -f /tmp/p2.sql; }
stage_c(){ docker exec "$STAGE" psql -U postgres -d postgres -v ON_ERROR_STOP=1 -Atc "$1"; }

echo "== backup =="
docker exec -e PGPASSWORD="$PGPASSWORD" "$PROD" pg_dump -U qalaatal -d qalaat-db -Fc -f /tmp/qalaat-p2.dump
docker cp "$PROD:/tmp/qalaat-p2.dump" "$BACKUP_DIR/qalaat-pre-phase2.dump"
sha256sum "$BACKUP_DIR/qalaat-pre-phase2.dump" | tee "$BACKUP_DIR/sha256.txt"
[[ "$(prod_c "SELECT count(*) FROM \"Users\" WHERE lower(\"Username\")='ahmed'")" == "1" ]]

echo "== schema extras =="
cat > "$SQLDIR/01_schema.sql" <<'SQL'
ALTER TABLE "Warehouses" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Warehouses_ImportKey" ON "Warehouses" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "Products" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Products_ImportKey" ON "Products" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "ProductCategories" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_ProductCategories_ImportKey" ON "ProductCategories" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "Purchases" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Purchases_ImportKey" ON "Purchases" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "PurchaseItems" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_PurchaseItems_ImportKey" ON "PurchaseItems" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "OrderLists" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_OrderLists_ImportKey" ON "OrderLists" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "Transactions" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Transactions_ImportKey" ON "Transactions" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

ALTER TABLE "Users" ADD COLUMN IF NOT EXISTS "ImportKey" varchar(120) NULL;
CREATE UNIQUE INDEX IF NOT EXISTS "UX_Users_ImportKey" ON "Users" ("ImportKey") WHERE "ImportKey" IS NOT NULL;

CREATE TABLE IF NOT EXISTS najaf_import.boxes (
  import_key text PRIMARY KEY,
  source_box_id int NOT NULL,
  name text NOT NULL,
  box_state boolean,
  payload jsonb NOT NULL
);
CREATE TABLE IF NOT EXISTS najaf_import.transfers (
  import_key text PRIMARY KEY,
  payload jsonb NOT NULL
);
SQL
prod_file "$SQLDIR/01_schema.sql"

RATE=$(stage_c "SELECT (payload->>'Amount')::float8 FROM source_stage.rows WHERE source_table='dbo.DollarAmount' ORDER BY 1 LIMIT 1")
RATE=${RATE:-1448}
echo "RATE=$RATE BRANCH_ID=$BRANCH_ID"

echo "== export =="
stage_c "COPY (
  SELECT payload->>'StoreID', left(coalesce(payload->>'StoreName',''),50), coalesce(nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.Stores'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/stores.csv"

stage_c "COPY (
  SELECT payload->>'ItemID', left(coalesce(payload->>'ItemName',''),100),
         coalesce(payload->>'Quantity','0'),
         coalesce(payload->>'ItemCost','0'),
         coalesce(payload->>'ItemPrice','0'),
         coalesce(payload->>'AmountDay','0'),
         coalesce(payload->>'StoreID','1'),
         left(coalesce(payload->>'Notes',''),400),
         coalesce(nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.Items'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/items.csv"

stage_c "COPY (
  SELECT payload->>'BuyID', coalesce(payload->>'BoundNumber', payload->>'BuyID'),
         coalesce(payload->>'StoreID','1'),
         coalesce(nullif(payload->>'DateCreate',''), nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.Buys'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/buys.csv"

stage_c "COPY (
  SELECT payload->>'BuyItemID', payload->>'BuyID', payload->>'ItemID', coalesce(payload->>'Quantity','0'),
         coalesce(nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.BuysItems'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/buy_items.csv"

stage_c "COPY (
  SELECT payload->>'DelegateID',
         left(coalesce(nullif(payload->>'DelegateName',''), nullif(payload->>'ReceiptName',''), concat('قائمة-',payload->>'DelegateID')),50),
         left(coalesce(payload->>'ReceiptName',''),50),
         left(coalesce(payload->>'PhoneNumber',''),30),
         coalesce(nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.Delegates'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/delegates.csv"

stage_c "COPY (
  SELECT payload->>'BoxID', left(coalesce(payload->>'BoxName',''),50), coalesce(payload->>'BoxState','true'), payload::text
  FROM source_stage.rows WHERE source_table='dbo.Boxes'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/boxes.csv"

stage_c "COPY (
  SELECT payload->>'WithdrawalFromBoxID',
         coalesce(payload->>'Amount','0'),
         left(coalesce(payload->>'Purpose', payload->>'Notes',''),400),
         coalesce(nullif(payload->>'DateCreate',''), nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows WHERE source_table='dbo.WithdrawalFromBox'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/withdrawals.csv"

stage_c "COPY (
  SELECT coalesce(payload->>'AddToBoxID', payload->>'CustomerPaymentID'),
         coalesce(payload->>'Amount','0'),
         left(coalesce(payload->>'Notes',''),400),
         coalesce(nullif(payload->>'DateCreate',''), nullif(payload->>'CreatedDate',''), now()::text)
  FROM source_stage.rows
  WHERE source_table='dbo.View_AddToBox'
    AND payload->>'CustomerIDPayment' IS NULL
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/box_in_nocust.csv"

stage_c "COPY (
  SELECT payload->>'TransferBoxID', payload::text
  FROM source_stage.rows WHERE source_table='dbo.TransferBoxs'
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/transfers.csv"

wc -l "$WORKDIR"/*.csv | tee "$BACKUP_DIR/export_counts.txt"

SAFE_ID=$(prod_c "SELECT \"Id\" FROM \"Safes\" WHERE \"BranchId\"=$BRANCH_ID LIMIT 1")
echo "SAFE_ID=$SAFE_ID"

echo "== category + branch warehouse (smart UI map) =="
# Qalaat UI: one warehouse per branch (CreateBranch seed). Do NOT paste raw dbo.Stores as extra warehouses.
WH_FALLBACK=$(prod_c "SELECT \"Id\" FROM \"Warehouses\" WHERE \"BranchId\"=$BRANCH_ID ORDER BY \"Id\" LIMIT 1")
cat > "$SQLDIR/02_wh.sql" <<SQL
UPDATE "Warehouses"
SET "Name" = 'مخزن النجف',
    "ImportKey" = coalesce("ImportKey", 'najaf|Stores|default')
WHERE "Id" = $WH_FALLBACK AND "BranchId" = $BRANCH_ID;

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

-- all source Stores collapse onto the branch default warehouse
INSERT INTO najaf_import.map(import_key, entity_type, entity_id, source_table)
SELECT concat('najaf|Stores|', s.src_id), 'Warehouse', $WH_FALLBACK, 'Stores'
FROM (VALUES ('1'),('2'),('3'),('default')) AS s(src_id)
ON CONFLICT (import_key) DO UPDATE SET entity_id = EXCLUDED.entity_id;
SQL
prod_file "$SQLDIR/02_wh.sql"

CAT_ID=$(prod_c "SELECT \"Id\" FROM \"ProductCategories\" WHERE \"ImportKey\"='najaf|Cat|other'")
echo "CAT_ID=$CAT_ID WH_FALLBACK=$WH_FALLBACK"

echo "== products =="
docker cp "$WORKDIR/items.csv" "$PROD:/tmp/items.csv"
cat > "$SQLDIR/03_products.sql" <<SQL
CREATE TEMP TABLE raw_items(
  item_id text, name text, qty text, cost text, price text, day_amt text, store_id text, notes text, created text
);
COPY raw_items FROM '/tmp/items.csv' WITH (FORMAT csv, NULL '');

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
    ELSE $CAT_ID
  END,
  $WH_FALLBACK,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|Items|', item_id)
FROM raw_items
WHERE NOT EXISTS (SELECT 1 FROM "Products" p WHERE p."ImportKey"=concat('najaf|Items|', raw_items.item_id));

-- ensure already-imported rows also sit on branch warehouse
UPDATE "Products"
SET "WarehouseId" = $WH_FALLBACK, "LastUpdatedAt" = now()
WHERE "ImportKey" LIKE 'najaf|Items|%' AND "WarehouseId" IS DISTINCT FROM $WH_FALLBACK;

INSERT INTO najaf_import.map(import_key, entity_type, entity_id, source_table)
SELECT p."ImportKey", 'Product', p."Id", 'Items'
FROM "Products" p WHERE p."ImportKey" LIKE 'najaf|Items|%'
ON CONFLICT DO NOTHING;
SQL
prod_file "$SQLDIR/03_products.sql"

echo "== purchases =="
docker cp "$WORKDIR/buys.csv" "$PROD:/tmp/buys.csv"
docker cp "$WORKDIR/buy_items.csv" "$PROD:/tmp/buy_items.csv"
cat > "$SQLDIR/04_purchases.sql" <<SQL
CREATE TEMP TABLE raw_buys(buy_id text, bound text, store_id text, created text);
COPY raw_buys FROM '/tmp/buys.csv' WITH (FORMAT csv, NULL '');
CREATE TEMP TABLE raw_bi(bi_id text, buy_id text, item_id text, qty text, created text);
COPY raw_bi FROM '/tmp/buy_items.csv' WITH (FORMAT csv, NULL '');

WITH totals AS (
  SELECT b.buy_id,
    coalesce(sum(coalesce(nullif(bi.qty,''),'0')::float8 * coalesce(nullif(it.payload->>'ItemCost',''),'0')::float8),0) * $RATE AS total
  FROM raw_buys b
  LEFT JOIN raw_bi bi ON bi.buy_id=b.buy_id
  LEFT JOIN source_stage.rows it ON false
  GROUP BY b.buy_id
)
SELECT 1;
SQL
# purchases totals computed in prod without stage join — use items already imported
cat > "$SQLDIR/04_purchases.sql" <<SQL
CREATE TEMP TABLE raw_buys(buy_id text, bound text, store_id text, created text);
COPY raw_buys FROM '/tmp/buys.csv' WITH (FORMAT csv, NULL '');
CREATE TEMP TABLE raw_bi(bi_id text, buy_id text, item_id text, qty text, created text);
COPY raw_bi FROM '/tmp/buy_items.csv' WITH (FORMAT csv, NULL '');

CREATE TEMP TABLE buy_totals AS
SELECT b.buy_id,
  coalesce(sum(
    coalesce(nullif(bi.qty,''),'0')::float8 *
    coalesce((SELECT p."BuyAmount"/$RATE FROM "Products" p WHERE p."ImportKey"=concat('najaf|Items|', bi.item_id)),0)
  ),0) * $RATE AS total_iqd,
  min(b.bound) AS bound,
  min(b.created) AS created
FROM raw_buys b
LEFT JOIN raw_bi bi ON bi.buy_id=b.buy_id
GROUP BY b.buy_id;

INSERT INTO "Purchases" (
  "FactorNumber","TotalAmount","BranchId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","SafeType","OrderId","ImportKey"
)
SELECT
  greatest(1, floor(coalesce(nullif(bound,''),'0')::float8)::int),
  total_iqd,
  $BRANCH_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  0, NULL,
  concat('najaf|Buys|', buy_id)
FROM buy_totals t
WHERE NOT EXISTS (SELECT 1 FROM "Purchases" p WHERE p."ImportKey"=concat('najaf|Buys|', t.buy_id));

INSERT INTO "PurchaseItems" (
  "Quantity","Amount","ProductId","PurchaseId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ForeignProductName","ImportKey"
)
SELECT
  greatest(0, floor(coalesce(nullif(bi.qty,''),'0')::float8)::int),
  coalesce((SELECT p."BuyAmount" FROM "Products" p WHERE p."ImportKey"=concat('najaf|Items|', bi.item_id)),0),
  (SELECT p."Id" FROM "Products" p WHERE p."ImportKey"=concat('najaf|Items|', bi.item_id)),
  (SELECT pu."Id" FROM "Purchases" pu WHERE pu."ImportKey"=concat('najaf|Buys|', bi.buy_id)),
  bi.created::timestamptz,
  1, bi.created::timestamptz, 1,
  NULL,
  concat('najaf|BuysItems|', bi.bi_id)
FROM raw_bi bi
WHERE (SELECT pu."Id" FROM "Purchases" pu WHERE pu."ImportKey"=concat('najaf|Buys|', bi.buy_id)) IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM "PurchaseItems" x WHERE x."ImportKey"=concat('najaf|BuysItems|', bi.bi_id));

INSERT INTO najaf_import.map(import_key, entity_type, entity_id, source_table)
SELECT p."ImportKey", 'Purchase', p."Id", 'Buys' FROM "Purchases" p WHERE p."ImportKey" LIKE 'najaf|Buys|%'
ON CONFLICT DO NOTHING;
SQL
prod_file "$SQLDIR/04_purchases.sql"

echo "== order lists from delegates =="
docker cp "$WORKDIR/delegates.csv" "$PROD:/tmp/delegates.csv"
cat > "$SQLDIR/05_lists.sql" <<SQL
CREATE TEMP TABLE raw_del(del_id text, list_name text, receipt text, phone text, created text);
COPY raw_del FROM '/tmp/delegates.csv' WITH (FORMAT csv, NULL '');

-- mandob users (no login password; ImportKey set; do not touch ahmed)
INSERT INTO "Users" (
  "FullName","Username","PasswordHash","PhoneNumber","SecurityStamp","CreationStep",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","UndeliveredCashAmount","PermissionsVersion","ImportKey"
)
SELECT
  left(coalesce(nullif(receipt,''), list_name),50),
  left(concat('najaf_d_', del_id),100),
  NULL,
  left(coalesce(phone,''),30),
  gen_random_uuid(),
  3,
  created::timestamptz,
  1, created::timestamptz, 1,
  0, 0,
  concat('najaf|Delegates|', del_id)
FROM raw_del
WHERE NOT EXISTS (SELECT 1 FROM "Users" u WHERE u."ImportKey"=concat('najaf|Delegates|', raw_del.del_id))
  AND NOT EXISTS (SELECT 1 FROM "Users" u WHERE u."Username"=left(concat('najaf_d_', raw_del.del_id),100));

INSERT INTO "UserBranches" ("UserId","BranchId")
SELECT u."Id", $BRANCH_ID
FROM "Users" u
WHERE u."ImportKey" LIKE 'najaf|Delegates|%'
ON CONFLICT DO NOTHING;

INSERT INTO "UserRoles" ("UserId","RoleId")
SELECT u."Id", 7
FROM "Users" u
WHERE u."ImportKey" LIKE 'najaf|Delegates|%'
  AND NOT EXISTS (SELECT 1 FROM "UserRoles" ur WHERE ur."UserId"=u."Id" AND ur."RoleId"=7);

INSERT INTO "OrderLists" (
  "Name","MandobId","MotabaId","BranchId","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  left(list_name,50),
  u."Id",
  $MOTABA_ID,
  $BRANCH_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|Delegates|', del_id)
FROM raw_del
JOIN "Users" u ON u."ImportKey"=concat('najaf|Delegates|', raw_del.del_id)
WHERE NOT EXISTS (SELECT 1 FROM "OrderLists" ol WHERE ol."ImportKey"=concat('najaf|Delegates|', raw_del.del_id));

SQL
prod_file "$SQLDIR/05_lists.sql"

# Fix order list linking using customers' source delegate from stage export
stage_c "COPY (
  SELECT payload->>'CustomerID', payload->>'DelegateID'
  FROM source_stage.rows WHERE source_table='dbo.Customers' AND payload->>'DelegateID' IS NOT NULL
) TO STDOUT WITH (FORMAT csv, NULL '')" > "$WORKDIR/cust_del.csv"
docker cp "$WORKDIR/cust_del.csv" "$PROD:/tmp/cust_del.csv"
cat > "$SQLDIR/05b_link_orders.sql" <<'SQL'
CREATE TEMP TABLE raw_cd(cust_src text, del_src text);
COPY raw_cd FROM '/tmp/cust_del.csv' WITH (FORMAT csv, NULL '');

UPDATE "Orders" o
SET "OrderListId" = ol."Id"
FROM "Customers" c
JOIN raw_cd cd ON c."ImportKey" = concat('najaf|Customers|', cd.cust_src)
JOIN "OrderLists" ol ON ol."ImportKey" = concat('najaf|Delegates|', cd.del_src)
WHERE o."CustomerId" = c."Id"
  AND o."ImportKey" LIKE 'najaf|CustomersSales|%';
SQL
prod_file "$SQLDIR/05b_link_orders.sql"

echo "== boxes archive + safe txs =="
docker cp "$WORKDIR/boxes.csv" "$PROD:/tmp/boxes.csv"
docker cp "$WORKDIR/withdrawals.csv" "$PROD:/tmp/withdrawals.csv"
docker cp "$WORKDIR/box_in_nocust.csv" "$PROD:/tmp/box_in_nocust.csv"
docker cp "$WORKDIR/transfers.csv" "$PROD:/tmp/transfers.csv"
cat > "$SQLDIR/06_safe.sql" <<SQL
CREATE TEMP TABLE raw_boxes(box_id text, name text, state text, payload text);
COPY raw_boxes FROM '/tmp/boxes.csv' WITH (FORMAT csv, NULL '');
INSERT INTO najaf_import.boxes(import_key, source_box_id, name, box_state, payload)
SELECT concat('najaf|Boxes|', box_id), box_id::int, name, lower(state) IN ('true','t','1'), payload::jsonb
FROM raw_boxes
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE raw_transfers(xfer_id text, payload text);
COPY raw_transfers FROM '/tmp/transfers.csv' WITH (FORMAT csv, NULL '');
INSERT INTO najaf_import.transfers(import_key, payload)
SELECT concat('najaf|TransferBoxs|', xfer_id), payload::jsonb FROM raw_transfers
ON CONFLICT DO NOTHING;

CREATE TEMP TABLE raw_wd(id text, amount text, purpose text, created text);
COPY raw_wd FROM '/tmp/withdrawals.csv' WITH (FORMAT csv, NULL '');
INSERT INTO "Transactions" (
  "Amount","Type","Status","Direction","StatusDescription","SafeId",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  coalesce(nullif(amount,''),'0')::float8 * $RATE,
  2, -- Expense
  1, -- Approved
  1, -- Out
  left(purpose,400),
  $SAFE_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|WithdrawalFromBox|', id)
FROM raw_wd
WHERE $SAFE_ID IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM "Transactions" t WHERE t."ImportKey"=concat('najaf|WithdrawalFromBox|', raw_wd.id));

CREATE TEMP TABLE raw_in(id text, amount text, notes text, created text);
COPY raw_in FROM '/tmp/box_in_nocust.csv' WITH (FORMAT csv, NULL '');
INSERT INTO "Transactions" (
  "Amount","Type","Status","Direction","StatusDescription","SafeId",
  "CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy","ImportKey"
)
SELECT
  coalesce(nullif(amount,''),'0')::float8 * $RATE,
  0,
  1,
  0,
  left(notes,400),
  $SAFE_ID,
  created::timestamptz,
  1, created::timestamptz, 1,
  concat('najaf|AddToBoxNoCust|', id)
FROM raw_in
WHERE $SAFE_ID IS NOT NULL AND nullif(id,'') IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM "Transactions" t WHERE t."ImportKey"=concat('najaf|AddToBoxNoCust|', raw_in.id));

-- refresh branch safe totals from transactions (literal sum of imported txs)
UPDATE "Safes" s SET
  "DebitAmount" = coalesce((SELECT sum("Amount") FROM "Transactions" t WHERE t."SafeId"=s."Id" AND t."Direction"=1),0),
  "CreditAmount" = coalesce((SELECT sum("Amount") FROM "Transactions" t WHERE t."SafeId"=s."Id" AND t."Direction"=0),0),
  "RemainingCashAmount" = coalesce((SELECT sum(CASE WHEN t."Direction"=0 THEN t."Amount" ELSE -t."Amount" END) FROM "Transactions" t WHERE t."SafeId"=s."Id"),0),
  "LastUpdatedAt" = now(),
  "LastUpdatedBy" = 1
WHERE s."Id" = $SAFE_ID;
SQL
prod_file "$SQLDIR/06_safe.sql"

echo "== match =="
cat > "$SQLDIR/07_match.sql" <<'SQL'
SELECT 'warehouses' k, count(*)::text FROM "Warehouses" WHERE "ImportKey" LIKE 'najaf|Stores|%';
SELECT 'products' k, count(*)::text FROM "Products" WHERE "ImportKey" LIKE 'najaf|Items|%';
SELECT 'purchases' k, count(*)::text FROM "Purchases" WHERE "ImportKey" LIKE 'najaf|Buys|%';
SELECT 'purchase_items' k, count(*)::text FROM "PurchaseItems" WHERE "ImportKey" LIKE 'najaf|BuysItems|%';
SELECT 'order_lists' k, count(*)::text FROM "OrderLists" WHERE "ImportKey" LIKE 'najaf|Delegates|%';
SELECT 'orders_with_list' k, count(*)::text FROM "Orders" WHERE "ImportKey" LIKE 'najaf|CustomersSales|%' AND "OrderListId" IS NOT NULL;
SELECT 'boxes_archived' k, count(*)::text FROM najaf_import.boxes;
SELECT 'transfers_archived' k, count(*)::text FROM najaf_import.transfers;
SELECT 'safe_txs' k, count(*)::text FROM "Transactions" WHERE "ImportKey" LIKE 'najaf|%';
SELECT 'ahmed' k, count(*)::text FROM "Users" WHERE lower("Username")='ahmed';
SQL
prod_file "$SQLDIR/07_match.sql" | tee "$BACKUP_DIR/phase2_match.txt"

stage_c "SELECT concat('src_stores|',count(*)) FROM source_stage.rows WHERE source_table='dbo.Stores'" | tee "$BACKUP_DIR/src.txt"
stage_c "SELECT concat('src_items|',count(*)) FROM source_stage.rows WHERE source_table='dbo.Items'" | tee -a "$BACKUP_DIR/src.txt"
stage_c "SELECT concat('src_buys|',count(*)) FROM source_stage.rows WHERE source_table='dbo.Buys'" | tee -a "$BACKUP_DIR/src.txt"
stage_c "SELECT concat('src_buy_items|',count(*)) FROM source_stage.rows WHERE source_table='dbo.BuysItems'" | tee -a "$BACKUP_DIR/src.txt"
stage_c "SELECT concat('src_delegates|',count(*)) FROM source_stage.rows WHERE source_table='dbo.Delegates'" | tee -a "$BACKUP_DIR/src.txt"
stage_c "SELECT concat('src_boxes|',count(*)) FROM source_stage.rows WHERE source_table='dbo.Boxes'" | tee -a "$BACKUP_DIR/src.txt"
stage_c "SELECT concat('src_withdrawals|',count(*)) FROM source_stage.rows WHERE source_table='dbo.WithdrawalFromBox'" | tee -a "$BACKUP_DIR/src.txt"

echo "PHASE2_DONE backup=$BACKUP_DIR"
