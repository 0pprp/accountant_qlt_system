"""تحميل بيانات الزبون التفصيلية: المواقع والصور والمستندات والتقييمات وبروفايل المحل.

المصدر ملفات NDJSON المُستخرَجة + مجلد الصور المفكوك من السداسي.
كل جدول idempotent بفهرس ImportKey الفريد.
"""
import argparse
import json
import os
from datetime import datetime

import psycopg

SYS = -1


def rd(d, n):
    p = os.path.join(d, f"{n}.ndjson")
    if not os.path.exists(p):
        return []
    with open(p, encoding="utf-8") as f:
        return [json.loads(x) for x in f if x.strip()]


def ts(v):
    try:
        return datetime.fromisoformat((v or "").replace("Z", "+00:00"))
    except (ValueError, AttributeError):
        return None


def day(v):
    t = ts(v)
    return t.date() if t else None


def f(v):
    try:
        return float(v) if v is not None else None
    except (TypeError, ValueError):
        return None


def norm(s):
    return " ".join(str(s or "").split()) or None


def main(dsn, files, photos_dir):
    cn = psycopg.connect(dsn, autocommit=False)
    c = cn.cursor()
    c.execute("SELECT current_database()")
    assert c.fetchone()[0] == "qalaat_local"

    c.execute("SELECT import_key, entity_id FROM najaf_import.map WHERE entity_type='Customers'")
    cust = {k.rsplit("|", 1)[1]: v for k, v in c.fetchall()}
    c.execute("SELECT import_key, entity_id FROM najaf_import.map WHERE entity_type='Orders'")
    order = {k.rsplit("|", 1)[1]: v for k, v in c.fetchall()}
    print(f"زبائن معروفون: {len(cust):,} · مبيعات: {len(order):,}")

    def cid(v):
        return cust.get(str(v)) if v is not None else None

    def oid(v):
        return order.get(str(v)) if v is not None else None

    # جسر: SaleId في مسار طلبات المبيعات -> زبوننا، عبر SalesDrafts ثم عبر الهاتف
    phone_to_customer = {}
    c.execute('SELECT "PhoneNumber", "Id" FROM "Customers" WHERE "PhoneNumber" <> %s', ("",))
    for ph, pid in c.fetchall():
        digits = "".join(ch for ch in (ph or "") if ch.isdigit())
        if len(digits) >= 10:
            phone_to_customer.setdefault(digits[-10:], pid)

    def by_phone(v):
        digits = "".join(ch for ch in str(v or "") if ch.isdigit())
        return phone_to_customer.get(digits[-10:]) if len(digits) >= 10 else None

    sale_to_customer = {}
    for dft in rd(files, "drafts"):
        key = str(dft.get("SaleId"))
        target = cid(dft.get("CustomerId"))
        if target is None and dft.get("CustomerSaleId") is not None:
            c.execute('SELECT "CustomerId" FROM "Orders" WHERE "Id"=%s', (oid(dft["CustomerSaleId"]),))
            row = c.fetchone()
            target = row[0] if row else None
        if target is None:
            target = by_phone(dft.get("Phone"))
        if target is not None:
            sale_to_customer[key] = target

    def sale_cid(v):
        return sale_to_customer.get(str(v)) if v is not None else None

    done = {}

    # ---------- المواقع ----------
    c.execute('DELETE FROM "CustomerLocations" WHERE "ImportKey" LIKE %s', ("najaf|%",))
    rows = []
    for r in rd(files, "loc_capture"):
        k = cid(r.get("CustomerID"))
        if k and f(r.get("Latitude")):
            rows.append((k, 0, f(r["Latitude"]), f(r["Longitude"]), f(r.get("AccuracyM")),
                         None, ts(r.get("LocationAtUtc")), f"najaf|LocCapture|{r['CustomerID']}"))
    for r in rd(files, "loc_estimates"):
        k = cid(r.get("CustomerID"))
        if k and f(r.get("Latitude")):
            rows.append((k, 1, f(r["Latitude"]), f(r["Longitude"]), None,
                         f(r.get("Confidence")), ts(r.get("UpdatedAtUtc")),
                         f"najaf|LocEstimate|{r['CustomerID']}"))
    for r in rd(files, "shop_profiles"):
        k = cid(r.get("CustomerId")) or sale_cid(r.get("SaleId"))
        if k and f(r.get("Latitude")):
            rows.append((k, 2, f(r["Latitude"]), f(r["Longitude"]), None, None,
                         ts(r.get("CreatedAtUtc")), f"najaf|LocShop|{r['Id']}"))
    for r in rd(files, "legal_cases"):
        k = cid(r.get("CustomerId"))
        if k and f(r.get("Latitude")):
            rows.append((k, 3, f(r["Latitude"]), f(r["Longitude"]), None, None,
                         ts(r.get("CreatedAtUtc")), f"najaf|LocLegal|{r['Id']}"))
    with c.copy('COPY "CustomerLocations" ("CustomerId","Source","Latitude","Longitude",'
                '"AccuracyMeters","Confidence","CapturedAtUtc","ImportKey") FROM STDIN') as cp:
        for row in rows:
            cp.write_row(row)
    done["المواقع"] = len(rows)

    # ---------- الصور ----------
    c.execute('DELETE FROM "CustomerShopPhotos" WHERE "ImportKey" LIKE %s', ("najaf|%",))
    index = {str(r["CustomerID"]): r for r in rd(files, "photo_index")}
    n = 0
    for fn in sorted(os.listdir(photos_dir)) if os.path.isdir(photos_dir) else []:
        src_id = fn.rsplit(".", 1)[0]
        k = cid(src_id)
        if k is None:
            continue
        with open(os.path.join(photos_dir, fn), "rb") as fh:
            data = fh.read()
        meta = index.get(src_id, {})
        c.execute('INSERT INTO "CustomerShopPhotos" ("CustomerId","Content","ContentType",'
                  '"CapturedAtUtc","ImportKey") VALUES (%s,%s,%s,%s,%s)',
                  (k, data, norm(meta.get("ContentType")) or "image/jpeg",
                   ts(meta.get("UpdatedAtUtc")), f"najaf|ShopPhoto|{src_id}"))
        n += 1
    done["الصور"] = n

    # ---------- المستندات ----------
    c.execute('DELETE FROM "CustomerDocuments" WHERE "ImportKey" LIKE %s', ("najaf|%",))
    docs = []
    for r in rd(files, "cust_docs"):
        k = cid(r.get("CustomerId")) or sale_cid(r.get("SaleId"))
        if k and r.get("FileKey"):
            docs.append((k, oid(r.get("SaleId")), norm(r.get("DocumentType")) or "مستند",
                         r["FileKey"], norm(r.get("FileName")), norm(r.get("ContentType")),
                         ts(r.get("CreatedAtUtc")), f"najaf|CustDoc|{r['Id']}"))
    for r in rd(files, "sale_docs2"):
        k = sale_cid(r.get("SaleId"))
        if k and r.get("StoragePath"):
            docs.append((k, oid(r.get("SaleId")), norm(r.get("DocumentType")) or "مستند بيع",
                         r["StoragePath"], norm(r.get("FileName")), None,
                         ts(r.get("CreatedAt")), f"najaf|SaleDoc|{r['Id']}"))
    with c.copy('COPY "CustomerDocuments" ("CustomerId","OrderId","DocumentType","FileKey",'
                '"FileName","ContentType","CreatedAtUtc","ImportKey") FROM STDIN') as cp:
        for row in docs:
            cp.write_row(row)
    done["المستندات"] = len(docs)

    # ---------- التقييمات ----------
    c.execute('DELETE FROM "CustomerRatingSnapshots" WHERE "ImportKey" LIKE %s', ("najaf|%",))
    snaps = []
    for r in rd(files, "rating_bm"):
        k = cid(r.get("CustomerId"))
        d = day(r.get("CaptureDate")) or day(r.get("CapturedAtUtc"))
        if k and d:
            snaps.append((k, "BranchManager", norm(r.get("RatingLabel")), f(r.get("Score")),
                          d, ts(r.get("CapturedAtUtc")), f"najaf|RatingBm|{r['Id']}"))
    for r in rd(files, "rating_follower"):
        k = cid(r.get("CustomerId"))
        d = day(r.get("CaptureDate")) or day(r.get("CapturedAtUtc"))
        if k and d:
            snaps.append((k, "Follower", norm(r.get("RatingLabel")), f(r.get("Score")),
                          d, ts(r.get("CapturedAtUtc")), f"najaf|RatingFl|{r['Id']}"))
    with c.copy('COPY "CustomerRatingSnapshots" ("CustomerId","Origin","RatingLabel","Score",'
                '"CaptureDate","CapturedAtUtc","ImportKey") FROM STDIN') as cp:
        for row in snaps:
            cp.write_row(row)
    done["التقييمات"] = len(snaps)

    # ---------- بروفايل المحل ----------
    c.execute('DELETE FROM "CustomerShopProfiles" WHERE "ImportKey" LIKE %s', ("najaf|%",))
    profs = []
    for r in rd(files, "shop_profiles"):
        k = cid(r.get("CustomerId")) or sale_cid(r.get("SaleId"))
        if k:
            profs.append((k, oid(r.get("SaleId")), norm(r.get("ShopName")),
                          norm(r.get("ShopBusinessType")), f(r.get("ShopStockEstimatedValue")),
                          f(r.get("EstimatedDailyRevenue")), f(r.get("ShopLength")),
                          f(r.get("ShopWidth")), f(r.get("ShopArea")),
                          norm(r.get("ShopImageKey")), ts(r.get("CreatedAtUtc")),
                          f"najaf|ShopProfile|{r['Id']}"))
    with c.copy('COPY "CustomerShopProfiles" ("CustomerId","OrderId","ShopName","BusinessType",'
                '"StockEstimatedValue","EstimatedDailyRevenue","ShopLength","ShopWidth",'
                '"ShopArea","ShopImageKey","CreatedAtUtc","ImportKey") FROM STDIN') as cp:
        for row in profs:
            cp.write_row(row)
    done["بروفايل المحل"] = len(profs)

    # ---------- الملاحظات: دمج ملاحظات المبيعات ----------
    c.execute('SELECT "Id" FROM "Branches" WHERE "Name"=%s', ("فرع النجف",))
    branch = c.fetchone()[0]
    added = 0
    c.execute('DELETE FROM "CustomerNotes" WHERE "AuthorName" LIKE %s', ("najaf:%",))
    for r in rd(files, "sales_notes2") or rd(files, "sales_notes"):
        k = cid(r.get("CustomerId")) or by_phone(r.get("CustomerPhone"))
        txt = norm(r.get("Note"))
        if not k or not txt:
            continue
        c.execute('INSERT INTO "CustomerNotes" ("CustomerId","BranchId","NoteText","AuthorName",'
                  '"CreatedAtUtc") VALUES (%s,%s,%s,%s,%s)',
                  (k, branch, txt[:2000], (norm(r.get("AuthorName")) or "النجف")[:100],
                   ts(r.get("CreatedAtUtc")) or datetime(2020, 1, 1)))
        added += 1
    done["ملاحظات المبيعات"] = added

    # ---------- أفضل موقع على بطاقة الزبون ----------
    c.execute("""
        UPDATE "Customers" c
        SET "Business_GpsLatitude" = l."Latitude", "Business_GpsLongitude" = l."Longitude"
        FROM (SELECT DISTINCT ON ("CustomerId") "CustomerId","Latitude","Longitude"
              FROM "CustomerLocations" ORDER BY "CustomerId","Source") l
        WHERE c."Id" = l."CustomerId";
    """)
    cn.commit()

    for k, v in done.items():
        print(f"  {k}: {v:,}")
    c.execute('SELECT count(*) FROM "Customers" WHERE "Business_GpsLatitude" IS NOT NULL')
    print(f"  زبائن بإحداثيات: {c.fetchone()[0]:,}")
    cn.close()


if __name__ == "__main__":
    ap = argparse.ArgumentParser()
    ap.add_argument("--files", required=True)
    ap.add_argument("--photos", required=True)
    ap.add_argument("--dsn", default="host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres")
    a = ap.parse_args()
    main(a.dsn, a.files, a.photos)
