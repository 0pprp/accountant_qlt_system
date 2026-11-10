import sys
import psycopg2

key = sys.argv[1]
c = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="qalaat_22_23_iso",
    user="postgres",
    password="postgres",
)
cur = c.cursor()
cur.execute(
    'SELECT COUNT(*) FROM "InstallmentPayments" WHERE "ClientPaymentId"=%s',
    (key,),
)
print("KEY_COUNT", cur.fetchone()[0])
c.close()
