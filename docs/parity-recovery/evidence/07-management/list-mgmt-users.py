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
    SELECT u."Username", r."Name"
    FROM "Users" u
    JOIN "UserRoles" ur ON ur."UserId"=u."Id"
    JOIN "Roles" r ON r."Id"=ur."RoleId"
    WHERE r."Name" IN ('Director','Monitor','DelegatedManager','BranchManager','Admin')
    ORDER BY r."Name", u."Username"
    """
)
for row in cur.fetchall():
    print("|".join(str(x) for x in row))
c.close()
