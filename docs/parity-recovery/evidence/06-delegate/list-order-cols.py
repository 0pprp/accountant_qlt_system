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
    SELECT column_name
    FROM information_schema.columns
    WHERE table_name='Orders'
    ORDER BY ordinal_position
    """
)
print([r[0] for r in cur.fetchall()])
c.close()
