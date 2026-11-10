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
    """
    SELECT o."Id", o."SellerId", o."SellAmount", o."ExecutionStatus",
           COALESCE((SELECT SUM(p."Amount") FROM "InstallmentPayments" p WHERE p."OrderId"=o."Id"),0)
    FROM "Orders" o
    WHERE o."SellerId" IS NOT NULL
    ORDER BY o."Id"
    LIMIT 10
    """
)
for row in cur.fetchall():
    print("|".join(str(x) for x in row))
c.close()
