# -*- coding: utf-8 -*-
"""Copy admin password hash to gg and grant Task.* via UserPermissions for isolated tests."""
import psycopg

TASK_PERMS = [
    "Task.Read",
    "Task.Create",
    "Task.Assign",
    "Task.Accept",
    "Task.Reject",
    "Task.Complete",
    "Task.Confirm",
    "Task.Transfer",
    "Task.ReadAttachment",
]

with psycopg.connect(
    "host=localhost port=5432 dbname=qalaat_local user=postgres password=postgres"
) as c:
    with c.cursor() as cur:
        cur.execute('SELECT "PasswordHash" FROM "Users" WHERE "Id"=1')
        (admin_hash,) = cur.fetchone()
        cur.execute(
            'UPDATE "Users" SET "PasswordHash"=%s, "SecurityStamp"=gen_random_uuid() WHERE "Id"=2',
            (admin_hash,),
        )
        cur.execute(
            """
            SELECT p."Id", p."Name" FROM "Permissions" p
            WHERE p."Name" = ANY(%s)
            """,
            (TASK_PERMS,),
        )
        perms = cur.fetchall()
        print("FOUND_PERMS", perms)
        for pid, pname in perms:
            cur.execute(
                """
                INSERT INTO "UserPermissions" ("UserId", "PermissionId")
                SELECT 2, %s
                WHERE NOT EXISTS (
                    SELECT 1 FROM "UserPermissions" up
                    WHERE up."UserId"=2 AND up."PermissionId"=%s
                )
                """,
                (pid, pid),
            )
        # limited01: same password for login tests, but NO Task.* grants
        cur.execute(
            'UPDATE "Users" SET "PasswordHash"=%s, "SecurityStamp"=gen_random_uuid() WHERE "Id"=3',
            (admin_hash,),
        )
        c.commit()
        print("READY user gg password=admin123 with Task.*; limited01 password synced without Task.*")
