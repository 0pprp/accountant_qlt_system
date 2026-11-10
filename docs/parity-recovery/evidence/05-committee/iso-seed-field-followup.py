import psycopg2

c = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="qalaat_22_23_iso",
    user="postgres",
    password="postgres",
)
cur = c.cursor()

# Ensure Motaba list has an in-progress order + customer for field followup / payment tests.
cur.execute('SELECT "Id" FROM "Customers" ORDER BY "Id" LIMIT 1')
row = cur.fetchone()
if not row:
    cur.execute(
        """
INSERT INTO "Customers" ("FullName","PhoneNumber","CreatedAt","CreatedBy","ConcurrencyStamp")
VALUES ('زبون ميداني iso','07000000001', NOW(), 1, 0)
RETURNING "Id"
"""
    )
    customer_id = cur.fetchone()[0]
else:
    customer_id = row[0]

cur.execute(
    """
SELECT "Id" FROM "Orders"
WHERE "OrderListId"=1 AND "ExecutionStatus"=1
LIMIT 1
"""
)
ord_row = cur.fetchone()
if not ord_row:
    # Find any order on list 1 and force InProgress, or insert minimal.
    cur.execute('SELECT "Id" FROM "Orders" WHERE "OrderListId"=1 ORDER BY "Id" LIMIT 1')
    existing = cur.fetchone()
    if existing:
        cur.execute(
            'UPDATE "Orders" SET "ExecutionStatus"=1, "CustomerId"=%s WHERE "Id"=%s',
            (customer_id, existing[0]),
        )
        order_id = existing[0]
    else:
        cur.execute('SELECT "Id" FROM "Branches" ORDER BY "Id" LIMIT 1')
        branch_id = cur.fetchone()[0]
        cur.execute(
            """
INSERT INTO "Orders" (
  "BuyAmount","SellAmount","PrepaymentAmount","DailyInstallmentAmount",
  "Step","ExecutionStatus","ApprovalStatus","WorkflowStatus",
  "CustomerId","BranchId","OrderListId","CreatedAt","CreatedBy","ConcurrencyStamp"
) VALUES (
  100000, 200000, 0, 5000,
  2, 1, 1, 4,
  %s, %s, 1, NOW(), 1, 0
) RETURNING "Id"
""",
            (customer_id, branch_id),
        )
        order_id = cur.fetchone()[0]
else:
    order_id = ord_row[0]

c.commit()
print("CUSTOMER", customer_id, "ORDER", order_id)

cur.execute(
    'SELECT "Id","CustomerId","OrderListId","SellAmount","ExecutionStatus" FROM "Orders" WHERE "Id"=%s',
    (order_id,),
)
print("ORDER_ROW", cur.fetchone())
c.close()
