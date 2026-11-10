"""Ensure a second InProgress order for the same SellerId as order 1 (iso only)."""
import psycopg2

c = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="qalaat_22_23_iso",
    user="postgres",
    password="postgres",
)
cur = c.cursor()
cur.execute(
    'SELECT "SellerId", "CustomerId", "BranchId", "OrderListId" FROM "Orders" WHERE "Id"=1'
)
seller_id, customer_id, branch_id, list_id = cur.fetchone()

cur.execute(
    """
    SELECT o."Id" FROM "Orders" o
    WHERE o."SellerId"=%s AND o."Id"<>1 AND o."ExecutionStatus"=1 AND o."SellAmount">=2000
    ORDER BY o."Id" LIMIT 1
    """,
    (seller_id,),
)
row = cur.fetchone()
if row:
    print(f"EXISTING|{row[0]}|{seller_id}")
    c.close()
    raise SystemExit(0)

# Prefer reusing another order row for same customer.
cur.execute(
    """
    SELECT o."Id" FROM "Orders" o
    WHERE o."Id"<>1 AND o."CustomerId"=%s
    ORDER BY o."Id" LIMIT 1
    """,
    (customer_id,),
)
row = cur.fetchone()
if row:
    oid = row[0]
    cur.execute(
        """
        UPDATE "Orders"
        SET "SellerId"=%s, "OrderListId"=%s, "SellAmount"=500000,
            "ExecutionStatus"=1
        WHERE "Id"=%s
        """,
        (seller_id, list_id, oid),
    )
    c.commit()
    print(f"REUSED|{oid}|{seller_id}")
    c.close()
    raise SystemExit(0)

# Insert by cloning nullable-safe subset from order 1.
cur.execute(
    """
    SELECT column_name
    FROM information_schema.columns
    WHERE table_schema='public' AND table_name='Orders'
      AND column_name NOT IN ('Id')
      AND is_generated = 'NEVER'
    ORDER BY ordinal_position
    """
)
cols = [r[0] for r in cur.fetchall()]
col_list = ", ".join(f'"{c}"' for c in cols)
cur.execute(
    f"""
    INSERT INTO "Orders" ({col_list})
    SELECT {col_list} FROM "Orders" WHERE "Id"=1
    RETURNING "Id"
    """
)
oid = cur.fetchone()[0]
cur.execute(
    """
    UPDATE "Orders"
    SET "SellAmount"=500000, "ExecutionStatus"=1, "SellerId"=%s, "OrderListId"=%s
    WHERE "Id"=%s
    """,
    (seller_id, list_id, oid),
)
# wipe any cloned installment payments attached via cascade? payments reference OrderId — clone doesn't copy them.
c.commit()
print(f"INSERTED|{oid}|{seller_id}")
c.close()
