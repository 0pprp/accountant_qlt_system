"""تحميل تقييمات الموظفين الثلاثة في مواضعها الصحيحة.

كل تقييم التُقط في لحظة مختلفة من دورة الزبون:
  المتابع / مسؤول اللجنة -> عند المصادقة على البيع  (FollowerSaleAuthentications)
  المندوب                -> عند أول تسديد           (DelegateFirstCollectionEvaluations)

موظف المبيعات بلا تقييم رقمي: استمارته كشف محل تُعرض من CustomerShopProfiles.
و SalesRequests.ShopRating هو تقييم المندوب عند طلب مبيع جديد، ولا يُحتسب بقرار المالك.

المقياس في المصدر 1..10 وهو نفس مقياسنا، فلا تحويل.
"""
import argparse
import json
import os
from datetime import datetime, timezone

import psycopg

SYS = -1
ROLE_COMMITTEE = "CommitteeField"
ROLE_DELEGATE = "Delegate"


def rd(d, n):
    p = os.path.join(d, f"{n}.ndjson")
    if not os.path.exists(p):
        return []
    with open(p, encoding="utf-8") as f:
        return [json.loads(x) for x in f if x.strip()]


def ts(v):
    try:
        parsed = datetime.fromisoformat((v or "").replace("Z", "+00:00"))
    except (ValueError, AttributeError):
        return datetime(2020, 1, 1, tzinfo=timezone.utc)
    return parsed if parsed.tzinfo else parsed.replace(tzinfo=timezone.utc)


def f(v):
    try:
        return float(v) if v is not None else None
    except (TypeError, ValueError):
        return None


def score(v):
    try:
        n = int(round(float(v)))
    except (TypeError, ValueError):
        return None
    return n if 1 <= n <= 10 else None


def norm(s):
    return " ".join(str(s or "").split()) or None


def main(dsn, files):
    cn = psycopg.connect(dsn, autocommit=False)
    c = cn.cursor()
    c.execute("SELECT current_database()")
    assert c.fetchone()[0] == "qalaat_local"

    c.execute("SELECT import_key, entity_id FROM najaf_import.map WHERE entity_type='Customers'")
    cust = {k.rsplit("|", 1)[1]: v for k, v in c.fetchall()}
    c.execute("SELECT import_key, entity_id FROM najaf_import.map WHERE entity_type='Orders'")
    order = {k.rsplit("|", 1)[1]: v for k, v in c.fetchall()}

    phones = {}
    c.execute('SELECT "PhoneNumber", "Id" FROM "Customers" WHERE "PhoneNumber" <> %s', ("",))
    for ph, pid in c.fetchall():
        digits = "".join(ch for ch in (ph or "") if ch.isdigit())
        if len(digits) >= 10:
            phones.setdefault(digits[-10:], pid)

    def by_phone(v):
        digits = "".join(ch for ch in str(v or "") if ch.isdigit())
        return phones.get(digits[-10:]) if len(digits) >= 10 else None

    c.execute('DELETE FROM "CustomerStaffRatings"')

    rows = []   # (customerId, role, score, note, name, when, orderId, stock, revenue, ctx, occasion)

    for r in rd(files, "sale_auth"):
        cid = cust.get(str(r.get("CustomerId")))
        sc = score(r.get("Rating"))
        if cid and sc:
            rows.append((cid, ROLE_COMMITTEE, sc, norm(r.get("Note")),
                         norm(r.get("FollowerName")), ts(r.get("AuthenticatedAtUtc")),
                         order.get(str(r.get("CustomerSaleId"))),
                         f(r.get("ShopStockEstimatedValue")), f(r.get("EstimatedDailyRevenue")),
                         None, "Authentication"))

    for r in rd(files, "first_collection"):
        cid = cust.get(str(r.get("CustomerId")))
        sc = score(r.get("Rating"))
        if cid and sc:
            rows.append((cid, ROLE_DELEGATE, sc, norm(r.get("Note")),
                         norm(r.get("DelegateName")), ts(r.get("CreatedAtUtc")), None,
                         f(r.get("ShopStockEstimatedValue")), f(r.get("EstimatedDailyRevenue")),
                         f(r.get("SaleAmount")), "FirstCollection"))

    # تقييم واحد لكل (زبون, دور) — نُبقي الأحدث
    best = {}
    for row in rows:
        key = (row[0], row[1])
        if key not in best or row[5] > best[key][5]:
            best[key] = row

    inserted = 0
    for row in best.values():
        cid, role, sc, note, name, when, oid, stock, rev, ctx, occasion = row
        c.execute(
            'INSERT INTO "CustomerStaffRatings" ("CustomerId","Role","Score","Note",'
            '"RatedByUserId","RatedByName","RatedAtUtc","OrderId","ShopStockEstimatedValue",'
            '"EstimatedDailyRevenue","ContextAmount","Occasion") '
            'VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)',
            (cid, role, sc, note, SYS, name, when, oid, stock, rev, ctx, occasion))
        inserted += 1
    cn.commit()

    print(f"مُدخَل: {inserted} من {len(rows)} صفاً خاماً")
    c.execute('SELECT "Role", count(*), round(avg("Score"))::int FROM "CustomerStaffRatings" GROUP BY 1 ORDER BY 1')
    for role, n, avg in c.fetchall():
        label = {"CommitteeField": "مسؤول اللجنة / المتابع",
                 "Delegate": "المندوب"}.get(role, role)
        print(f"  {label}: {n} تقييماً، متوسط {avg}/10")
    cn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--files", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    a = ap.parse_args()
    main(a.dsn, a.files)
