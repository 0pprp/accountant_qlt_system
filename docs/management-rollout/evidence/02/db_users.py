# -*- coding: utf-8 -*-
import psycopg

with psycopg.connect(
    "host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres"
) as c:
    with c.cursor() as cur:
        cur.execute('SELECT "Id", "Username", "FullName" FROM "Users" ORDER BY "Id" LIMIT 30')
        print("USERS", cur.fetchall())
        cur.execute('SELECT "UserId", "BranchId" FROM "UserBranches" ORDER BY "UserId" LIMIT 40')
        print("BRANCHES", cur.fetchall())
        cur.execute(
            """
            SELECT p."Name" FROM "Permissions" p
            WHERE p."Name" LIKE 'Task.%'
            ORDER BY p."Name"
            """
        )
        print("TASK_PERMS", cur.fetchall())
