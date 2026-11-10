import sys
import psycopg2

enabled = sys.argv[1].lower() in ("1", "true", "on", "yes")
c = psycopg2.connect(
    host="localhost",
    port=5432,
    dbname="qalaat_22_23_iso",
    user="postgres",
    password="postgres",
)
cur = c.cursor()
cur.execute(
    'UPDATE "OrderLists" SET "FollowerCollectionEnabled"=%s WHERE "Id"=1',
    (enabled,),
)
c.commit()
print("COLLECTION_ENABLED" if enabled else "COLLECTION_DISABLED")
c.close()
