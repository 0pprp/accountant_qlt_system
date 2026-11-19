"""تحميل لقطة النجف إلى qalaat_local.

قراءة من ملفات NDJSON المُستخرَجة، وكتابة في القاعدة المحلية وحدها.
كل خطوة معاملة واحدة، والتكرار يُمنع بجدول najaf_import.map.

التشغيل: python load.py --files <dir> [--clear] [--load] [--verify]
"""
import argparse
import json
import os
import sys
from datetime import date, datetime

import psycopg
from psycopg.rows import dict_row

BRANCH_NAME = "فرع النجف"
PROVINCE_NAME = "النجف"
SYS = -1
EPS = 0.005


def read(files_dir, name):
    path = os.path.join(files_dir, f"{name}.ndjson")
    out = []
    with open(path, encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line:
                out.append(json.loads(line))
    return out


def dt(value, fallback=None):
    if not value:
        return fallback
    try:
        return datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError:
        return fallback


def d(value, fallback=date(2020, 1, 1)):
    parsed = dt(value)
    return parsed.date() if parsed else fallback


def s(value, limit, fallback=""):
    if value is None:
        return fallback
    text = str(value).strip()
    return (text[:limit] if text else fallback)


def num(value):
    try:
        return float(value or 0)
    except (TypeError, ValueError):
        return 0.0


SCHEMA = """
CREATE SCHEMA IF NOT EXISTS najaf_import;
CREATE TABLE IF NOT EXISTS najaf_import.map (
  import_key text PRIMARY KEY,
  entity_type text NOT NULL,
  entity_id bigint NOT NULL,
  created_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS najaf_import.steps (
  step text PRIMARY KEY, rows_in bigint, rows_out bigint,
  completed_at timestamptz NOT NULL DEFAULT now());
CREATE TABLE IF NOT EXISTS najaf_import.notes (
  id bigserial PRIMARY KEY, step text, detail text,
  created_at timestamptz NOT NULL DEFAULT now());
"""

CLEAR = """
TRUNCATE TABLE
  "SafeMovements", "Transactions", "Safes",
  "InstallmentPayments", "OrderItem", "Orders", "Customers",
  "PurchaseItems", "Purchases", "ExpenseItems", "Expenses",
  "Attachments", "CustomerNotes", "CustomerDecisions", "CustomerStaffRatings",
  "CustomerWaiverRequests", "LegalFollowupCaseDocuments", "LegalFollowupCases",
  "LegalConversionRequests", "WrongSaleNotices",
  "FollowerCustomerRatingSnapshots", "FollowerSaleAuthentications",
  "FollowerFieldFollowups", "FollowerDelegateNotes", "FollowerDelegateDayRatings",
  "DirectorBoardOperatorListRatings", "DirectorSalesEmployeeBoardRatings",
  "SalesEmployeeListDayRatings", "ListSupervisionDayRatings",
  "SalesRequestTimelineEntries", "SalesRequestChangeOutbox", "SalesRequests",
  "SalesEmployeeAssignments", "SalesEmployeeAppBlocks", "SalesShifts", "SalesLocationPoints",
  "ManagementTaskEvents", "ManagementTaskAttachments",
  "ManagementTaskChangeOutboxes", "ManagementTasks",
  "OrderLists", "Products", "ProductCategories", "Warehouses",
  "BranchVisitAttachments", "BranchVisitMembers", "BranchVisitRatings", "BranchVisits",
  "ComplaintMessages", "AppNotes", "ActivityLogs", "Notifications",
  "UserLogins", "UserPermissions", "UserPermissionDenials", "UserRoles", "UserBranches"
RESTART IDENTITY;
DELETE FROM "Users" WHERE "Username" IS DISTINCT FROM 'admin' AND "Username" IS DISTINCT FROM 'ahmed';
DELETE FROM "Branches" WHERE "Id" <> 1;
DROP SCHEMA IF EXISTS najaf_import CASCADE;
"""


class Loader:
    def __init__(self, conn, files_dir):
        self.c = conn
        self.dir = files_dir
        self.map = {}

    def remember(self, key, entity, entity_id):
        self.map[key] = entity_id
        self.c.execute(
            'INSERT INTO najaf_import.map (import_key, entity_type, entity_id) VALUES (%s,%s,%s)'
            ' ON CONFLICT (import_key) DO NOTHING',
            (key, entity, entity_id))

    def load_map(self):
        self.c.execute("SELECT import_key, entity_id FROM najaf_import.map")
        self.map = {r[0]: r[1] for r in self.c.fetchall()}

    def step(self, name, rows_in, rows_out):
        self.c.execute(
            'INSERT INTO najaf_import.steps (step, rows_in, rows_out) VALUES (%s,%s,%s)'
            ' ON CONFLICT (step) DO UPDATE SET rows_in=EXCLUDED.rows_in,'
            ' rows_out=EXCLUDED.rows_out, completed_at=now()',
            (name, rows_in, rows_out))
        print(f"  {name}: {rows_out} / {rows_in}")

    # ---------- الخطوات ----------

    def branch(self):
        self.c.execute('SELECT "Id" FROM "Provinces" WHERE "Name"=%s', (PROVINCE_NAME,))
        row = self.c.fetchone()
        if row:
            province_id = row[0]
        else:
            self.c.execute('INSERT INTO "Provinces" ("Name") VALUES (%s) RETURNING "Id"',
                           (PROVINCE_NAME,))
            province_id = self.c.fetchone()[0]

        self.c.execute('SELECT "Id" FROM "Branches" WHERE "Name"=%s', (BRANCH_NAME,))
        row = self.c.fetchone()
        if row:
            self.branch_id = row[0]
        else:
            self.c.execute(
                'INSERT INTO "Branches" ("Name","ProvinceId","CreatedAt","LastUpdatedAt",'
                '"CreatedBy","LastUpdatedBy") VALUES (%s,%s,now(),now(),%s,%s) RETURNING "Id"',
                (BRANCH_NAME, province_id, SYS, SYS))
            self.branch_id = self.c.fetchone()[0]
        self.step("branch", 1, 1)

    def users(self):
        rows = read(self.dir, "delegates")
        made = 0
        for r in rows:
            key = f"najaf|Delegates|{r['DelegateID']}"
            if key in self.map:
                continue
            name = s(r.get("DelegateName"), 50, f"مندوب {r['DelegateID']}")
            self.c.execute(
                'INSERT INTO "Users" ("FullName","PhoneNumber","Address","SecurityStamp",'
                '"CreationStep","UndeliveredCashAmount","PermissionsVersion","CreatedAt",'
                '"CreatedBy","LastUpdatedAt","LastUpdatedBy") '
                'VALUES (%s,%s,%s,gen_random_uuid(),2,0,0,now(),%s,now(),%s) RETURNING "Id"',
                (name, s(r.get("PhoneNumber"), 30, None), s(r.get("Address"), 400, None), SYS, SYS))
            self.remember(key, "Users", self.c.fetchone()[0])
            made += 1

        # حساب نظامي لعمود Products.CreatedBy الذي عليه مفتاح أجنبي
        key = "najaf|System|importer"
        if key not in self.map:
            self.c.execute(
                'INSERT INTO "Users" ("FullName","SecurityStamp","CreationStep",'
                '"UndeliveredCashAmount","PermissionsVersion","CreatedAt","CreatedBy",'
                '"LastUpdatedAt","LastUpdatedBy") '
                'VALUES (%s,gen_random_uuid(),2,0,0,now(),%s,now(),%s) RETURNING "Id"',
                ("نظام استيراد النجف", SYS, SYS))
            self.remember(key, "Users", self.c.fetchone()[0])
            made += 1
        self.importer_id = self.map["najaf|System|importer"]

        for uid in {self.map[k] for k in self.map if k.startswith("najaf|Delegates|")}:
            self.c.execute(
                'INSERT INTO "UserBranches" ("UserId","BranchId") VALUES (%s,%s)'
                ' ON CONFLICT DO NOTHING', (uid, self.branch_id))
        self.step("users", len(rows), made)

    def orderlists(self):
        rows = read(self.dir, "delegates")
        made = 0
        for r in rows:
            key = f"najaf|List|{r['DelegateID']}"
            dkey = f"najaf|Delegates|{r['DelegateID']}"
            if key in self.map or dkey not in self.map:
                continue
            mandob = self.map[dkey]
            name = s(r.get("DelegateName"), 50, f"قائمة {r['DelegateID']}")
            self.c.execute(
                'INSERT INTO "OrderLists" ("Name","MandobId","MotabaId","BranchId",'
                '"CreatedAt","LastUpdatedAt","CreatedBy","LastUpdatedBy",'
                '"FollowerCollectionEnabled","CollectionToggleVersion") '
                'VALUES (%s,%s,%s,%s,now(),now(),%s,%s,%s,0) RETURNING "Id"',
                (name, mandob, mandob, self.branch_id, SYS, SYS,
                 bool(r.get("FollowerCollectionEnabled"))))
            self.remember(key, "OrderLists", self.c.fetchone()[0])
            made += 1
        self.step("orderlists", len(rows), made)

    def warehouses(self):
        rows = read(self.dir, "stores")
        made = 0
        for r in rows:
            key = f"najaf|Stores|{r['StoreID']}"
            if key in self.map:
                continue
            self.c.execute(
                'INSERT INTO "Warehouses" ("Name","BranchId") VALUES (%s,%s) RETURNING "Id"',
                (s(r.get("StoreName"), 50, f"مخزن {r['StoreID']}"), self.branch_id))
            self.remember(key, "Warehouses", self.c.fetchone()[0])
            made += 1
        self.step("warehouses", len(rows), made)

    def products(self):
        rows = read(self.dir, "items")
        key = "najaf|Cat|general"
        if key not in self.map:
            self.c.execute(
                'INSERT INTO "ProductCategories" ("Name","CreatedAt","LastUpdatedAt",'
                '"CreatedBy","LastUpdatedBy") VALUES (%s,now(),now(),%s,%s) RETURNING "Id"',
                ("مواد النجف", SYS, SYS))
            self.remember(key, "ProductCategories", self.c.fetchone()[0])
        category_id = self.map[key]

        made = 0
        for r in rows:
            k = f"najaf|Items|{r['ItemID']}"
            if k in self.map:
                continue
            wh = self.map.get(f"najaf|Stores|{r.get('StoreID')}")
            self.c.execute(
                'INSERT INTO "Products" ("Name","RemainingCount","BuyAmount","SellAmount",'
                '"DailyInstallmentAmount","CategoryId","WarehouseId","CreatedAt","CreatedBy",'
                '"LastUpdatedAt","LastUpdatedBy") '
                'VALUES (%s,%s,%s,%s,%s,%s,%s,now(),%s,now(),%s) RETURNING "Id"',
                (s(r.get("ItemName"), 100, f"مادة {r['ItemID']}"), int(r.get("Quantity") or 0),
                 num(r.get("ItemCostDenar")), num(r.get("ItemPriceDenar")),
                 num(r.get("AmountDayDenar")), category_id, wh,
                 self.importer_id, SYS))
            self.remember(k, "Products", self.c.fetchone()[0])
            made += 1
        self.step("products", len(rows), made)

    def customers(self):
        rows = read(self.dir, "customers")
        made = 0
        for r in rows:
            key = f"najaf|Customers|{r['CustomerID']}"
            if key in self.map:
                continue
            lat = r.get("CapLat") or r.get("EstLat")
            lng = r.get("CapLng") or r.get("EstLng")
            phone = s(r.get("PhoneNumber"), 30, "")
            self.c.execute(
                'INSERT INTO "Customers" ("FullName","MotherName","NationalCode","BirthDate",'
                '"PhoneNumber","WhatsAppPhoneNumber","Business_Name","Business_Address",'
                '"Business_NearestKnownLocation","Business_GpsLatitude","Business_GpsLongitude",'
                '"BranchId","IsLegal","IsFakeSale","SellAmountDiscount","DailyInstallmentDiscount",'
                '"ImportKey","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
                'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,0,0,%s,%s,%s,now(),%s) '
                'RETURNING "Id"',
                (s(r.get("CustomerName"), 50, f"زبون {r['CustomerID']}"), "", "",
                 date(1970, 1, 1), phone, phone,
                 s(r.get("ShopName"), 100, "—"),
                 s(r.get("StoreAddress") or r.get("Address"), 300, "—"),
                 s(r.get("NearestFunctionPoint") or r.get("Neighborhood"), 200, "—"),
                 float(lat) if lat else None, float(lng) if lng else None,
                 self.branch_id, bool(r.get("IsLegal")), bool(r.get("IsFakeSale")),
                 key, dt(r.get("CreatedDate")), SYS, SYS))
            self.remember(key, "Customers", self.c.fetchone()[0])
            made += 1
        self.step("customers", len(rows), made)

    def orders(self):
        rows = read(self.dir, "sales")
        made = 0
        for r in rows:
            key = f"najaf|CustomersSales|{r['CustomerSaleID']}"
            if key in self.map:
                continue
            cust = self.map.get(f"najaf|Customers|{r.get('CustomerID')}")
            if cust is None:
                continue
            seller = self.map.get(f"najaf|Delegates|{r.get('DelegateID')}")
            lst = self.map.get(f"najaf|List|{r.get('DelegateID')}")
            sell = num(r.get("AmountTotalSalesDenar"))
            created = dt(r.get("DateCreate"))
            self.c.execute(
                'INSERT INTO "Orders" ("BuyAmount","SellAmount","PrepaymentAmount",'
                '"DailyInstallmentAmount","Step","ExecutionStatus","ApprovalStatus",'
                '"WorkflowStatus","CustomerId","BranchId","SellerId","OrderListId","SaleDate",'
                '"SalesCompletedAtUtc","SellAmountDiscountApplied","DailyInstallmentDiscountApplied",'
                '"ImportKey","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
                'VALUES (%s,%s,0,%s,2,%s,1,5,%s,%s,%s,%s,%s,%s,0,0,%s,%s,%s,now(),%s) RETURNING "Id"',
                (num(r.get("AmountTotalCostDenar")), sell, num(r.get("AmountDaySalesDenar")),
                 2 if num(r.get("AmountRemaining")) <= EPS else 1,
                 cust, self.branch_id, seller, lst, d(r.get("DateCreate")),
                 created, key, created, SYS, SYS))
            self.remember(key, "Orders", self.c.fetchone()[0])
            made += 1
        self.step("orders", len(rows), made)

    def order_items(self):
        rows = read(self.dir, "sale_items")
        made = 0
        for r in rows:
            order = self.map.get(f"najaf|CustomersSales|{r.get('CustomerSaleID')}")
            if order is None:
                continue
            key = f"najaf|SelectItemsSales|{r['SelectItemsSaleID']}"
            if key in self.map:
                continue
            qty = int(r.get("Quantity") or 1)
            self.c.execute(
                'INSERT INTO "OrderItem" ("ProductType","Quantity","BuyAmount","SellAmount",'
                '"PrepaymentAmount","DailyInstallmentAmount","OrderId","ProductName","ProductId",'
                '"CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
                'VALUES (0,%s,%s,%s,0,%s,%s,%s,%s,now(),%s,now(),%s) RETURNING "Id"',
                (qty, num(r.get("ItemCostDenar")) * qty, num(r.get("ItemPriceDenar")) * qty,
                 num(r.get("AmountDayDenar")) * qty, order,
                 s(r.get("ItemName"), 200, None),
                 self.map.get(f"najaf|Items|{r.get('ItemID')}"), SYS, SYS))
            self.remember(key, "OrderItem", self.c.fetchone()[0])
            made += 1
        self.step("order_items", len(rows), made)

    def payments(self):
        rows = read(self.dir, "payments")
        self.c.execute(
            'SELECT "CustomerId", "Id", "SellAmount" FROM "Orders"'
            ' WHERE "ImportKey" LIKE %s ORDER BY "CustomerId", "SaleDate", "Id"',
            ("najaf|CustomersSales|%",))
        by_customer = {}
        for cid, oid, sell in self.c.fetchall():
            by_customer.setdefault(cid, []).append([oid, float(sell), 0.0])

        made = 0
        unallocated = 0
        for r in rows:
            key = f"najaf|CustomersPayments|{r['CustomerPaymentID']}"
            if key in self.map:
                continue
            cust = self.map.get(f"najaf|Customers|{r.get('CustomerID')}")
            amount = num(r.get("AmountDenar"))
            order_id = None
            buckets = by_customer.get(cust) or []
            for bucket in buckets:
                if bucket[2] + amount <= bucket[1] + EPS:
                    order_id = bucket[0]
                    bucket[2] += amount
                    break
            if order_id is None:
                if len(buckets) == 1:
                    order_id = buckets[0][0]
                    buckets[0][2] += amount
                else:
                    unallocated += 1
            self.c.execute(
                'INSERT INTO "InstallmentPayments" ("Amount","Date","OrderId","CustomerId",'
                '"Description","ImportKey","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
                'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,now(),%s) RETURNING "Id"',
                (amount, d(r.get("PaymentDate")), order_id, cust, "najaf", key,
                 dt(r.get("PaymentDate")), SYS, SYS))
            self.remember(key, "InstallmentPayments", self.c.fetchone()[0])
            made += 1
        self.c.execute('INSERT INTO najaf_import.notes (step, detail) VALUES (%s,%s)',
                       ("payments", f"غير مخصّصة لطلب: {unallocated}"))
        self.step("payments", len(rows), made)


def run_clear(dsn):
    with psycopg.connect(dsn, autocommit=False) as conn:
        cur = conn.cursor()
        cur.execute("SELECT current_database()")
        assert cur.fetchone()[0] == "qalaat_local", "الهدف لازم qalaat_local"
        cur.execute(CLEAR)
        conn.commit()
    print("تم تنظيف البيانات الوهمية")


def run_load(dsn, files_dir):
    with psycopg.connect(dsn, autocommit=False) as conn:
        cur = conn.cursor()
        cur.execute("SELECT current_database()")
        assert cur.fetchone()[0] == "qalaat_local", "الهدف لازم qalaat_local"
        cur.execute(SCHEMA)
        conn.commit()
        ld = Loader(cur, files_dir)
        ld.load_map()
        for fn in (ld.branch, ld.users, ld.orderlists, ld.warehouses, ld.products,
                   ld.customers, ld.orders, ld.order_items, ld.payments):
            fn()
            conn.commit()
    print("اكتمل التحميل")


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--files", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    ap.add_argument("--clear", action="store_true")
    ap.add_argument("--load", action="store_true")
    args = ap.parse_args()
    if args.clear:
        run_clear(args.dsn)
    if args.load:
        run_load(args.dsn, args.files)
