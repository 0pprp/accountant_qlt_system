"""تحميل حركات صناديق النجف إلى خزائن qalaat_local.

لا يُدرج خزينة واحدة — SafeProvisioner أنشأها عند الإقلاع، وهذا يكتب الحركات عليها فقط.
السحوبات والنقل بالدولار في المصدر فتُضرب بسعر الصرف؛ الإيداعات والتحصيلات بالدينار أصلاً.
أرجل النقل تُقرأ من TransferBoxs وحدها، وتُستثنى من الإيداعات والسحوبات منعاً للازدواج.
"""
import argparse
import json
import os
from datetime import datetime

import psycopg

RATE = 1448.0
SYS = -1
BRANCH_NAME = "فرع النجف"

KIND_OPENING, KIND_DEPOSIT, KIND_WITHDRAWAL = 0, 1, 2
KIND_TRANSFER_IN, KIND_TRANSFER_OUT, KIND_COLLECTION = 3, 4, 5


def read(d, n):
    with open(os.path.join(d, f"{n}.ndjson"), encoding="utf-8") as f:
        return [json.loads(x) for x in f if x.strip()]


def dt(v):
    if not v:
        return None
    try:
        return datetime.fromisoformat(v.replace("Z", "+00:00"))
    except ValueError:
        return None


def num(v):
    try:
        return float(v or 0)
    except (TypeError, ValueError):
        return 0.0


def main(dsn, files):
    conn = psycopg.connect(dsn, autocommit=False)
    cur = conn.cursor()
    cur.execute("SELECT current_database()")
    assert cur.fetchone()[0] == "qalaat_local", "الهدف لازم qalaat_local"

    cur.execute('SELECT "Id" FROM "Branches" WHERE "Name"=%s', (BRANCH_NAME,))
    branch_id = cur.fetchone()[0]
    cur.execute('SELECT "Id" FROM "Safes" WHERE "BranchId"=%s AND "Kind"=0', (branch_id,))
    branch_safe = cur.fetchone()[0]

    cur.execute("SELECT import_key, entity_id FROM najaf_import.map")
    imap = {k: v for k, v in cur.fetchall()}

    # صندوق المصدر → خزينة الهدف، عبر المندوب وقائمته
    cur.execute('SELECT "OrderListId", "Id" FROM "Safes" WHERE "Kind"=2')
    list_safe = {k: v for k, v in cur.fetchall()}

    box_to_safe = {}
    for dlg in read(files, "delegates"):
        box = dlg.get("BoxID")
        lst = imap.get(f"najaf|List|{dlg['DelegateID']}")
        if box and lst and lst in list_safe:
            box_to_safe[int(box)] = list_safe[lst]

    def safe_of(box):
        return box_to_safe.get(int(box)) if box else None

    moves = []          # (safe_id, kind, signed_amount, note, when, import_key, group, counterparty)
    source_delta = {}   # safe_id -> expected balance from the source

    def add(safe_id, kind, amount, note, when, key, group=None, counter=None):
        if safe_id is None:
            safe_id = branch_safe
            note = (note or "") + " [صندوق بلا مقابل — وُجّه لقاصة الفرع]"
        moves.append((safe_id, kind, amount, note, when, key, group, counter))
        source_delta[safe_id] = source_delta.get(safe_id, 0.0) + amount

    for p in read(files, "payments"):
        box = p.get("BoxIDActual") or p.get("BoxID")
        add(safe_of(box), KIND_COLLECTION, num(p.get("AmountDenar")), "تحصيل",
            dt(p.get("PaymentDate")), f"najaf|AddToBox|{p['AddToBoxID']}")

    for b in read(files, "box_deposits"):
        if b.get("TransferBoxID"):
            continue
        add(safe_of(b.get("BoxID")), KIND_DEPOSIT, num(b.get("AmountDenar")), "إيداع",
            dt(b.get("DateCreate")), f"najaf|AddToBox|{b['AddToBoxID']}")

    for w in read(files, "box_withdrawals"):
        if w.get("TransferBoxID"):
            continue
        add(safe_of(w.get("BoxID")), KIND_WITHDRAWAL, -abs(num(w.get("Amount")) * RATE),
            (w.get("Purpose") or "سحب")[:500], dt(w.get("DateCreate")),
            f"najaf|WithdrawalFromBox|{w['WithdrawalFromBoxID']}")

    for t in read(files, "box_transfers"):
        amount = abs(num(t.get("Amount")) * RATE)
        src, dst = safe_of(t.get("FromBoxID")), safe_of(t.get("ToBoxID"))
        gid = f"najaf-transfer-{t['TransferBoxID']}"
        when = dt(t.get("DateCreate"))
        add(src, KIND_TRANSFER_OUT, -amount, "نقل", when,
            f"najaf|TransferBoxs|out|{t['TransferBoxID']}", gid, dst or branch_safe)
        add(dst, KIND_TRANSFER_IN, amount, "نقل", when,
            f"najaf|TransferBoxs|in|{t['TransferBoxID']}", gid, src or branch_safe)

    print(f"حركات محسوبة: {len(moves):,}")

    cur.execute('DELETE FROM "SafeMovements" WHERE "ImportKey" LIKE %s', ("najaf|%",))
    with cur.copy(
        'COPY "SafeMovements" ("SafeId","Kind","Amount","BalanceAfter","Note",'
        '"PerformedByUserId","TransferGroupId","CounterpartySafeId","ImportKey",'
        '"CreatedAt","CreatedBy","LastUpdatedAt","LastUpdatedBy") FROM STDIN'
    ) as cp:
        for safe_id, kind, amount, note, when, key, group, counter in moves:
            gid = None
            if group:
                import uuid
                gid = str(uuid.uuid5(uuid.NAMESPACE_URL, group))
            cp.write_row((safe_id, kind, amount, 0.0, (note or "")[:500], SYS, gid, counter,
                          key, when, SYS, when, SYS))

    # الرصيد الجاري الصحيح زمنياً، ثم الرصيد المخزون
    cur.execute("""
        WITH ordered AS (
          SELECT "Id", SUM("Amount") OVER (PARTITION BY "SafeId"
                 ORDER BY "CreatedAt" NULLS FIRST, "Id"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS running
          FROM "SafeMovements")
        UPDATE "SafeMovements" m SET "BalanceAfter" = o.running FROM ordered o WHERE o."Id" = m."Id";
    """)
    cur.execute("""
        UPDATE "Safes" s SET "Balance" =
          COALESCE((SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"), 0);
    """)
    conn.commit()

    cur.execute("""
        SELECT count(*) FROM "Safes" s
        WHERE abs(s."Balance" - COALESCE(
          (SELECT SUM(m."Amount") FROM "SafeMovements" m WHERE m."SafeId"=s."Id"),0)) > 0.001;
    """)
    print(f"خزائن غير متوازنة: {cur.fetchone()[0]}")
    cur.execute('SELECT count(*), to_char(SUM("Amount"),%s) FROM "SafeMovements" WHERE "ImportKey" LIKE %s',
                ("FM999,999,999,999", "najaf|%"))
    n, total = cur.fetchone()
    print(f"حركات مُدرَجة: {n:,} — صافيها {total}")
    conn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--files", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    main(ap.parse_args().dsn, ap.parse_args().files)
