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
SELECT u."Id", u."Username", r."Name"
FROM "Users" u
JOIN "UserRoles" ur ON ur."UserId"=u."Id"
JOIN "Roles" r ON r."Id"=ur."RoleId"
WHERE r."Name" ILIKE '%Motaba%' OR r."Name" ILIKE '%متابع%'
LIMIT 15
"""
)
print("MOTABA_USERS", cur.fetchall())
cur.execute(
    """
SELECT ol."Id", ol."Name", ol."MotabaId", ol."MandobId", ol."FollowerCollectionEnabled"
FROM "OrderLists" ol
WHERE ol."MotabaId" IS NOT NULL
LIMIT 15
"""
)
print("LISTS", cur.fetchall())
cur.execute(
    """
SELECT o."Id", o."CustomerId", o."OrderListId", o."SellAmount", o."ExecutionStatus"
FROM "Orders" o
WHERE o."ExecutionStatus" = 1
LIMIT 10
"""
)
print("ORDERS_INPROGRESS", cur.fetchall())
c.close()
