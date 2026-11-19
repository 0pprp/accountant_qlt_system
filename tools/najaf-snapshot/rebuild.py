"""إعادة بناء بنية النجف وفق قرارات المالك وملف Box.xlsx.

- القوائم: السبع ذات التسديد + «قائمة أرشيف» لبقية المناطق
- الموظفون: المتابع حسين حيدر عبد السادة وحده
- متفرقة: كل صندوق ذي مال لا يتبع قائمة ولا موظفاً
- الصفر في الجدول لا يُنشأ
- البائع: ضياء فالح حسن وحده
- المواد: المخزن ١ فقط
"""
import argparse
import json
import os
import uuid
from datetime import datetime

import openpyxl
import psycopg

SYS = -1
RATE = 1448.0
BRANCH = "فرع النجف"
SELLER = "ضياء فالح حسن"
MOTABA = "حسين حيدر عبد السادة"
ARCHIVE_LIST = "قائمة أرشيف"

# اسم الصندوق في Box.xlsx -> اسم القائمة عندنا
LIST_BOXES = {
    "الولاية نجف 2": "الولاية نجف 2",
    "خزينة الشمالية": "الشمالية",
    "خزينة الكوفة": "الكوفة",
    "خزينة الوسطى": "الوسطى",
    "خزينة الولاية 1": "الولاية نجف 1",
    "خزيتة المشخاب 2": "المشخاب 2",
    "خزينة الكفل": "الكفل",
}
BRANCH_BOX = "خزينة النجف"
EXPENSES_BOX = "صرفيات النجف"

KIND_BRANCH, KIND_EXPENSES, KIND_LIST, KIND_EMPLOYEE, KIND_MISC = 0, 1, 2, 3, 4
K_OPENING, K_DEPOSIT, K_WITHDRAWAL, K_IN, K_OUT, K_COLLECTION = 0, 1, 2, 3, 4, 5


def norm(s):
    return " ".join(str(s or "").split())


def read_box(path):
    wb = openpyxl.load_workbook(path, read_only=True, data_only=True)
    ws = wb[wb.sheetnames[0]]
    it = ws.iter_rows(values_only=True)
    next(it)
    out = []
    for name, amount, state in ((r + (None, None, None))[:3] for r in it):
        if name is None:
            continue
        money = amount if isinstance(amount, (int, float)) and not isinstance(amount, bool) else 0.0
        out.append((norm(name), float(money)))
    wb.close()
    return out


def read_nd(d, n):
    with open(os.path.join(d, f"{n}.ndjson"), encoding="utf-8") as f:
        return [json.loads(x) for x in f if x.strip()]


def dt(v):
    try:
        return datetime.fromisoformat((v or "").replace("Z", "+00:00"))
    except (ValueError, AttributeError):
        return None


def num(v):
    try:
        return float(v or 0)
    except (TypeError, ValueError):
        return 0.0


def main(dsn, files, box_xlsx):
    boxes_xl = read_box(box_xlsx)
    funded = {n: a for n, a in boxes_xl if a > 0}
    print(f"صناديق ذات مال في الملف: {len(funded)}  مجموعها {sum(funded.values()):,.0f}")

    cn = psycopg.connect(dsn, autocommit=False)
    c = cn.cursor()
    c.execute("SELECT current_database()")
    assert c.fetchone()[0] == "qalaat_local"

    c.execute('SELECT "Id" FROM "Branches" WHERE "Name"=%s', (BRANCH,))
    branch_id = c.fetchone()[0]

    # ---- تنظيف البنية القديمة مع إبقاء الزبائن والمبيعات والتسديدات ----
    c.execute('DELETE FROM "SafeMovements"')
    c.execute('UPDATE "Orders" SET "OrderListId"=NULL, "SellerId"=NULL')
    c.execute('DELETE FROM "Safes" WHERE "BranchId"=%s', (branch_id,))
    c.execute('DELETE FROM "OrderLists" WHERE "BranchId"=%s', (branch_id,))
    c.execute("DELETE FROM najaf_import.map WHERE entity_type IN ('OrderLists','Safes')")
    cn.commit()

    # ---- المستخدمون: متابع واحد + نائب لكل قائمة ----
    def ensure_user(full_name, key):
        c.execute("SELECT entity_id FROM najaf_import.map WHERE import_key=%s", (key,))
        row = c.fetchone()
        if row:
            return row[0]
        c.execute(
            'INSERT INTO "Users" ("FullName","SecurityStamp","CreationStep","UndeliveredCashAmount",'
            '"PermissionsVersion","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
            'VALUES (%s,gen_random_uuid(),2,0,0,now(),%s,now(),%s) RETURNING "Id"',
            (full_name[:50], SYS, SYS))
        uid = c.fetchone()[0]
        c.execute('INSERT INTO najaf_import.map (import_key, entity_type, entity_id) VALUES (%s,%s,%s)',
                  (key, "Users", uid))
        c.execute('INSERT INTO "UserBranches" ("UserId","BranchId") VALUES (%s,%s) ON CONFLICT DO NOTHING',
                  (uid, branch_id))
        return uid

    motaba_id = ensure_user(MOTABA, "najaf|Motaba|hussein")
    seller_id = ensure_user(SELLER, "najaf|Seller|dhiaa")

    list_names = list(LIST_BOXES.values()) + [ARCHIVE_LIST]
    list_ids = {}
    for name in list_names:
        mandob = ensure_user(f"مندوب {name}", f"najaf|Mandob|{name}")
        c.execute(
            'INSERT INTO "OrderLists" ("Name","MandobId","MotabaId","BranchId","CreatedAt",'
            '"LastUpdatedAt","CreatedBy","LastUpdatedBy","FollowerCollectionEnabled",'
            '"CollectionToggleVersion") VALUES (%s,%s,%s,%s,now(),now(),%s,%s,false,0) RETURNING "Id"',
            (name[:50], mandob, motaba_id, branch_id, SYS, SYS))
        list_ids[name] = c.fetchone()[0]
    print(f"قوائم: {len(list_ids)} (منها قائمة أرشيف)")

    # ---- الخزائن ----
    def add_safe(name, kind, order_list_id=None, user_id=None, is_system=False):
        c.execute(
            'INSERT INTO "Safes" ("Name","Kind","BranchId","OrderListId","UserId","Balance",'
            '"IsSystem","IsActive","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
            'VALUES (%s,%s,%s,%s,%s,0,%s,true,now(),%s,now(),%s) RETURNING "Id"',
            (name[:100], kind, branch_id, order_list_id, user_id, is_system, SYS, SYS))
        return c.fetchone()[0]

    branch_safe = add_safe(f"قاصة {BRANCH}", KIND_BRANCH, is_system=True)
    expenses_safe = add_safe(f"سجل الصرفيات — {BRANCH}", KIND_EXPENSES, is_system=True)
    list_safes = {n: add_safe(n, KIND_LIST, order_list_id=i) for n, i in list_ids.items()}
    employee_safe = add_safe(MOTABA, KIND_EMPLOYEE, user_id=motaba_id)

    box_to_safe = {BRANCH_BOX: branch_safe, EXPENSES_BOX: expenses_safe}
    for box_name, list_name in LIST_BOXES.items():
        box_to_safe[box_name] = list_safes[list_name]

    misc = 0
    for name, amount in funded.items():
        if name in box_to_safe:
            continue
        box_to_safe[name] = add_safe(name, KIND_MISC)
        misc += 1
    cn.commit()
    print(f"خزائن: فرع 1 · صرفيات 1 · قوائم {len(list_safes)} · موظف 1 · متفرقة {misc}")

    # ---- ربط المبيعات بالقوائم والبائع ----
    srcbox = {int(b["BoxID"]): norm(b.get("BoxName")) for b in read_nd(files, "boxes")}
    dlg_box = {d["DelegateID"]: int(d["BoxID"]) for d in read_nd(files, "delegates") if d.get("BoxID")}
    dlg_name = {d["DelegateID"]: norm(d.get("DelegateName")) for d in read_nd(files, "delegates")}

    name_to_list = {}
    for did, dname in dlg_name.items():
        bx = srcbox.get(dlg_box.get(did, -1), "")
        target = LIST_BOXES.get(bx)
        name_to_list[did] = list_ids[target] if target else list_ids[ARCHIVE_LIST]

    c.execute("SELECT import_key, entity_id FROM najaf_import.map WHERE entity_type='Orders'")
    omap = dict(c.fetchall())
    linked = {}
    for s in read_nd(files, "sales"):
        oid = omap.get(f"najaf|CustomersSales|{s['CustomerSaleID']}")
        if oid is None:
            continue
        linked[oid] = (name_to_list.get(s.get("DelegateID"), list_ids[ARCHIVE_LIST]),
                       seller_id if s.get("UserID") == 7 else None)
    for oid, (lid, sid) in linked.items():
        c.execute('UPDATE "Orders" SET "OrderListId"=%s, "SellerId"=%s WHERE "Id"=%s', (lid, sid, oid))
    cn.commit()
    print(f"مبيعات مربوطة: {len(linked):,}")

    # ---- الحركات ----
    def resolve(box_id):
        return box_to_safe.get(srcbox.get(int(box_id), ""), branch_safe) if box_id else branch_safe

    moves = []

    def add(safe, kind, amt, note, when, key, group=None, counter=None):
        moves.append((safe, kind, amt, (note or "")[:500], when, key, group, counter))

    for p in read_nd(files, "payments"):
        add(resolve(p.get("BoxIDActual") or p.get("BoxID")), K_COLLECTION,
            num(p.get("AmountDenar")), "تحصيل", dt(p.get("PaymentDate")),
            f"najaf|AddToBox|{p['AddToBoxID']}")
    for b in read_nd(files, "box_deposits"):
        if b.get("TransferBoxID"):
            continue
        add(resolve(b.get("BoxID")), K_DEPOSIT, num(b.get("AmountDenar")), "إيداع",
            dt(b.get("DateCreate")), f"najaf|AddToBox|{b['AddToBoxID']}")
    for w in read_nd(files, "box_withdrawals"):
        if w.get("TransferBoxID"):
            continue
        add(resolve(w.get("BoxID")), K_WITHDRAWAL, -abs(num(w.get("Amount")) * RATE),
            w.get("Purpose") or "سحب", dt(w.get("DateCreate")),
            f"najaf|WithdrawalFromBox|{w['WithdrawalFromBoxID']}")
    for t in read_nd(files, "box_transfers"):
        a = abs(num(t.get("Amount")) * RATE)
        src, dst = resolve(t.get("FromBoxID")), resolve(t.get("ToBoxID"))
        g = str(uuid.uuid5(uuid.NAMESPACE_URL, f"najaf-transfer-{t['TransferBoxID']}"))
        w = dt(t.get("DateCreate"))
        add(src, K_OUT, -a, "نقل", w, f"najaf|TransferBoxs|out|{t['TransferBoxID']}", g, dst)
        add(dst, K_IN, a, "نقل", w, f"najaf|TransferBoxs|in|{t['TransferBoxID']}", g, src)

    with c.copy('COPY "SafeMovements" ("SafeId","Kind","Amount","BalanceAfter","Note",'
                '"PerformedByUserId","TransferGroupId","CounterpartySafeId","ImportKey",'
                '"CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") FROM STDIN') as cp:
        for safe, kind, amt, note, when, key, g, counter in moves:
            cp.write_row((safe, kind, amt, 0.0, note, SYS, g, counter, key, when, SYS, when, SYS))

    c.execute("""WITH o AS (SELECT "Id", SUM("Amount") OVER (PARTITION BY "SafeId"
                 ORDER BY "CreatedAt" NULLS FIRST, "Id"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) r FROM "SafeMovements")
                 UPDATE "SafeMovements" m SET "BalanceAfter"=o.r FROM o WHERE o."Id"=m."Id";""")
    c.execute("""UPDATE "Safes" s SET "Balance"=COALESCE(
                 (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0);""")
    cn.commit()
    print(f"حركات: {len(moves):,}")

    # ---- المواد: المخزن ١ فقط ----
    c.execute('SELECT "Id" FROM "Warehouses" WHERE "BranchId"=%s ORDER BY "Id" LIMIT 1', (branch_id,))
    wh = c.fetchone()
    c.execute('DELETE FROM "OrderItem" WHERE "ProductId" IS NOT NULL')
    c.execute('DELETE FROM "Products"')
    c.execute("DELETE FROM najaf_import.map WHERE entity_type='Products'")
    c.execute('SELECT "Id" FROM "ProductCategories" ORDER BY "Id" LIMIT 1')
    cat = c.fetchone()
    if cat is None:
        c.execute('INSERT INTO "ProductCategories" ("Name","CreatedAt","LastUpdatedAt","CreatedBy",'
                  '"LastUpdatedBy") VALUES (%s,now(),now(),%s,%s) RETURNING "Id"',
                  ("مواد النجف", SYS, SYS))
        cat = c.fetchone()
    c.execute('SELECT entity_id FROM najaf_import.map WHERE import_key=%s', ("najaf|System|importer",))
    importer = c.fetchone()[0]
    kept = 0
    for it in read_nd(files, "items"):
        if int(it.get("StoreID") or 0) != 1:
            continue
        c.execute('INSERT INTO "Products" ("Name","RemainingCount","BuyAmount","SellAmount",'
                  '"DailyInstallmentAmount","CategoryId","WarehouseId","CreatedAt","CreatedBy",'
                  '"LastUpdatedAt","LastUpdatedBy") VALUES (%s,%s,%s,%s,%s,%s,%s,now(),%s,now(),%s)',
                  (norm(it.get("ItemName"))[:100], int(it.get("Quantity") or 0),
                   round(num(it.get("ItemCostDenar"))), round(num(it.get("ItemPriceDenar"))),
                   round(num(it.get("AmountDayDenar"))), cat[0], wh[0] if wh else None,
                   importer, SYS))
        kept += 1
    cn.commit()
    print(f"مواد: {kept}")

    # ---- التحقق ----
    c.execute("""SELECT count(*) FROM "Safes" s WHERE abs(s."Balance"-COALESCE(
                 (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0))>0.001""")
    print(f"خزائن غير متوازنة: {c.fetchone()[0]}")
    c.execute('SELECT "Kind", count(*), SUM("Balance") FROM "Safes" WHERE "BranchId"=%s GROUP BY 1 ORDER BY 1',
              (branch_id,))
    for k, n, bal in c.fetchall():
        print(f"  Kind {k}: {n} خزينة  رصيد {float(bal):>16,.0f}")
    cn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--files", required=True)
    ap.add_argument("--box", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    a = ap.parse_args()
    main(a.dsn, a.files, a.box)
