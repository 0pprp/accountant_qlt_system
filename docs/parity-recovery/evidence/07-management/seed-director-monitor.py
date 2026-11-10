"""Iso-only: ensure director_iso / monitor_iso exist (clone password from bm_iso)."""
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
    SELECT column_name FROM information_schema.columns
    WHERE table_name='Users' ORDER BY ordinal_position
    """
)
cols = [r[0] for r in cur.fetchall()]
print("USER_COLS", ",".join(cols[:40]))

cur.execute('SELECT "Id","PasswordHash" FROM "Users" WHERE "Username"=%s', ("bm_iso",))
src = cur.fetchone()
if not src:
    raise SystemExit("bm_iso missing")
uid, pwd = src

# Clone bm_iso row then rename — safer than guessing required columns
for username, role_name in (("director_iso", "Director"), ("monitor_iso", "Monitor")):
    cur.execute('SELECT "Id" FROM "Users" WHERE "Username"=%s', (username,))
    row = cur.fetchone()
    if row:
        user_id = row[0]
        print(f"EXIST|{username}|{user_id}")
    else:
        copy_cols = [c for c in cols if c != "Id"]
        col_list = ", ".join(f'"{c}"' for c in copy_cols)
        cur.execute(
            f"""
            INSERT INTO "Users" ({col_list})
            SELECT {col_list} FROM "Users" WHERE "Id"=%s
            RETURNING "Id"
            """,
            (uid,),
        )
        user_id = cur.fetchone()[0]
        cur.execute(
            'UPDATE "Users" SET "Username"=%s, "FullName"=%s, "PasswordHash"=%s WHERE "Id"=%s',
            (username, f"{role_name} iso", pwd, user_id),
        )
        print(f"INSERTED|{username}|{user_id}")

    cur.execute('SELECT "Id" FROM "Roles" WHERE "Name"=%s', (role_name,))
    role = cur.fetchone()
    if not role:
        print(f"MISSING_ROLE|{role_name}")
        continue
    role_id = role[0]
    # replace roles with only target role for clean defaults test
    cur.execute('DELETE FROM "UserRoles" WHERE "UserId"=%s', (user_id,))
    cur.execute(
        'INSERT INTO "UserRoles" ("UserId","RoleId") VALUES (%s,%s)',
        (user_id, role_id),
    )
    print(f"ROLE_SET|{username}|{role_name}")

    cur.execute(
        """
        INSERT INTO "UserBranches" ("UserId","BranchId")
        SELECT %s, ub."BranchId" FROM "UserBranches" ub
        WHERE ub."UserId"=%s
          AND NOT EXISTS (
            SELECT 1 FROM "UserBranches" x WHERE x."UserId"=%s AND x."BranchId"=ub."BranchId"
          )
        """,
        (user_id, uid, user_id),
    )

c.commit()
c.close()
print("DONE")
