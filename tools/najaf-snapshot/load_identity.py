"""ملء حقول هوية الزبون المتاحة من استمارة البيع.

جدول زبائن النجف بلا رقم هوية ولا اسم أم ولا تاريخ ميلاد. لكن SalesDrafts
يحمل رقم البطاقة واسم المختار ومركز التموين لمن مرّ بمسار المبيعات.
من لا يملكها يبقى فارغاً صراحةً — لا قيمة وهمية.
"""
import argparse
import json
import os
import sys

import psycopg

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")


def rd(d, n):
    p = os.path.join(d, f"{n}.ndjson")
    if not os.path.exists(p):
        return []
    with open(p, encoding="utf-8") as f:
        return [json.loads(x) for x in f if x.strip()]


def norm(s):
    return " ".join(str(s or "").split()) or None


def digits(v):
    return "".join(ch for ch in str(v or "") if ch.isdigit())


def main(dsn, files):
    cn = psycopg.connect(dsn, autocommit=False)
    c = cn.cursor()
    c.execute("SELECT current_database()")
    assert c.fetchone()[0] == "qalaat_local"

    c.execute("SELECT import_key, entity_id FROM najaf_import.map WHERE entity_type='Customers'")
    cust = {k.rsplit("|", 1)[1]: v for k, v in c.fetchall()}

    phones = {}
    c.execute('SELECT "PhoneNumber", "Id" FROM "Customers" WHERE "PhoneNumber" <> %s', ("",))
    for ph, pid in c.fetchall():
        d = digits(ph)
        if len(d) >= 10:
            phones.setdefault(d[-10:], pid)

    drafts = rd(files, "drafts_identity") or rd(files, "drafts")
    matched = filled = 0
    unmatched = 0

    for r in drafts:
        national = norm(r.get("NationalCardNumber"))
        mukhtar = norm(r.get("MukhtarName"))
        ration = norm(r.get("RationCenterNumber"))
        if not (national or mukhtar or ration):
            continue

        target = cust.get(str(r.get("CustomerId")))
        if target is None:
            d = digits(r.get("Phone"))
            target = phones.get(d[-10:]) if len(d) >= 10 else None
        if target is None:
            unmatched += 1
            continue

        matched += 1
        c.execute(
            'UPDATE "Customers" SET '
            '"NationalCode" = CASE WHEN "NationalCode" = %s '
            'THEN COALESCE(%s::text, "NationalCode") ELSE "NationalCode" END, '
            '"MukhtarName" = COALESCE("MukhtarName", %s::text), '
            '"RationCenterNumber" = COALESCE("RationCenterNumber", %s::text) '
            'WHERE "Id" = %s',
            ("", national[:50] if national else None,
             mukhtar[:100] if mukhtar else None,
             ration[:50] if ration else None, target))
        filled += c.rowcount

    cn.commit()
    print(f"مسوّدات فيها هوية: {matched} · غير مطابقة: {unmatched}")

    c.execute('SELECT count(*) FILTER (WHERE "NationalCode" <> %s), '
              'count(*) FILTER (WHERE "MukhtarName" IS NOT NULL), '
              'count(*) FILTER (WHERE "RationCenterNumber" IS NOT NULL) FROM "Customers"', ("",))
    nat, muk, rat = c.fetchone()
    print(f"رقم الهوية: {nat} · المختار: {muk} · مركز التموين: {rat}")
    cn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--files", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    a = ap.parse_args()
    main(a.dsn, a.files)
