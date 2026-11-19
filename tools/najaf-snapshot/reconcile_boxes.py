"""تسوية أرصدة الخزائن على ملف Box.xlsx.

سجل الحركات المستورَد لا يجمع إلى الرصيد المعلن في الملف، لأن النظام القديم
يحتسب أرصدة أخرى لا تظهر في الجداول الأربعة المنقولة. فلكل خزينة حركة
«رصيد افتتاحي» واحدة مقدارها (الرصيد المعلن − مجموع الحركات)، فيصير
الرصيد المخزون مطابقاً للملف مع بقاء التاريخ كاملاً وثابت الدفتر سليماً.
"""
import argparse
import openpyxl
import psycopg

SYS = -1
BRANCH = "فرع النجف"
K_OPENING = 0
NOTE = "رصيد افتتاحي مُرحَّل — تسوية على كشف القاصات"


def norm(s):
    return " ".join(str(s or "").split())


def read_box(path):
    wb = openpyxl.load_workbook(path, read_only=True, data_only=True)
    ws = wb[wb.sheetnames[0]]
    it = ws.iter_rows(values_only=True)
    next(it)
    out = {}
    for row in it:
        name = row[0] if row else None
        if name is None:
            continue
        amt = row[1] if len(row) > 1 else None
        out[norm(name)] = float(amt) if isinstance(amt, (int, float)) and not isinstance(amt, bool) else 0.0
    wb.close()
    return out


def main(dsn, box_xlsx):
    target = read_box(box_xlsx)
    cn = psycopg.connect(dsn, autocommit=False)
    c = cn.cursor()
    c.execute("SELECT current_database()")
    assert c.fetchone()[0] == "qalaat_local"

    c.execute('SELECT "Id" FROM "Branches" WHERE "Name"=%s', (BRANCH,))
    branch_id = c.fetchone()[0]

    # اسم الصندوق في الملف لكل خزينة عندنا
    c.execute('SELECT "Id","Name","Kind" FROM "Safes" WHERE "BranchId"=%s ORDER BY "Kind","Id"',
              (branch_id,))
    safes = c.fetchall()

    LIST_BOX = {
        "الولاية نجف 2": "الولاية نجف 2", "الشمالية": "خزينة الشمالية",
        "الكوفة": "خزينة الكوفة", "الوسطى": "خزينة الوسطى",
        "الولاية نجف 1": "خزينة الولاية 1", "المشخاب 2": "خزيتة المشخاب 2",
        "الكفل": "خزينة الكفل",
    }

    def box_for(name, kind):
        if kind == 0:
            return "خزينة النجف"
        if kind == 1:
            return "صرفيات النجف"
        if kind == 2:
            return LIST_BOX.get(norm(name))
        if kind == 4:
            return norm(name)
        return None

    c.execute('DELETE FROM "SafeMovements" WHERE "ImportKey" LIKE %s', ("najaf|Opening|%",))

    rows = []
    unmatched = []
    for sid, name, kind in safes:
        box = box_for(name, kind)
        want = target.get(box) if box else 0.0
        if box and box not in target:
            unmatched.append((name, box))
            want = 0.0
        c.execute('SELECT COALESCE(SUM("Amount"),0) FROM "SafeMovements" WHERE "SafeId"=%s', (sid,))
        have = float(c.fetchone()[0])
        delta = round((want or 0.0) - have, 2)
        if abs(delta) > 0.001:
            rows.append((sid, delta))

    c.execute('SELECT MIN("CreatedAt") FROM "SafeMovements"')
    epoch = c.fetchone()[0]

    for sid, delta in rows:
        c.execute(
            'INSERT INTO "SafeMovements" ("SafeId","Kind","Amount","BalanceAfter","Note",'
            '"PerformedByUserId","ImportKey","CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") '
            'VALUES (%s,%s,%s,0,%s,%s,%s,%s,%s,%s,%s)',
            (sid, K_OPENING, delta, NOTE, SYS, f"najaf|Opening|{sid}", epoch, SYS, epoch, SYS))

    c.execute("""WITH o AS (SELECT "Id", SUM("Amount") OVER (PARTITION BY "SafeId"
                 ORDER BY "CreatedAt" NULLS FIRST, "Id"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) r FROM "SafeMovements")
                 UPDATE "SafeMovements" m SET "BalanceAfter"=o.r FROM o WHERE o."Id"=m."Id";""")
    c.execute("""UPDATE "Safes" s SET "Balance"=COALESCE(
                 (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0);""")
    cn.commit()

    print(f"حركات تسوية: {len(rows)}")
    if unmatched:
        print("أسماء بلا مقابل في الملف:", unmatched)

    c.execute('SELECT "Kind", count(*), SUM("Balance") FROM "Safes" WHERE "BranchId"=%s GROUP BY 1 ORDER BY 1',
              (branch_id,))
    total = 0.0
    for k, n, bal in c.fetchall():
        total += float(bal)
        print(f"  Kind {k}: {n:>3} خزينة  رصيد {float(bal):>16,.0f}")
    print(f"  المجموع              {total:>16,.0f}")
    print(f"  المعلن في الملف      {sum(target.values()):>16,.0f}")

    c.execute("""SELECT count(*) FROM "Safes" s WHERE abs(s."Balance"-COALESCE(
                 (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0))>0.001""")
    print(f"  خزائن غير متوازنة: {c.fetchone()[0]}")
    cn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--box", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    a = ap.parse_args()
    main(a.dsn, a.box)
