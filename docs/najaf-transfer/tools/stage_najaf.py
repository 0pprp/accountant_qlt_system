"""Load the read-only restored Najaf snapshot into a private PostgreSQL rehearsal.

Run locally after Restore-NajafSnapshot.ps1. Neither container publishes a port.
Financial values pass from SQL Server JSON text directly to PostgreSQL numeric JSON.
The original SQL Server backup remains the authority for schema and binary types.
"""
import hashlib
import json
import os
import secrets
import subprocess

SQL = "najaf-rehearsal-bluawosx"
PG = "najaf-pg-bluawosx"


def run(args, data=None):
    return subprocess.run(args, input=data, capture_output=True, check=True).stdout


def source_sql(query):
    return run(["docker", "exec", SQL, "sh", "-ec",
                'export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"; exec /opt/mssql-tools18/bin/sqlcmd -C -I -U sa -S localhost -b -h -1 -W -d NajafRehearsal -Q "$1"',
                "sh", "SET NOCOUNT ON; " + query]).decode("utf-8-sig").strip()


def target_sql(query):
    return run(["docker", "exec", "-i", PG, "psql", "-XqAt", "-v", "ON_ERROR_STOP=1",
                "-U", "postgres", "-d", "postgres"], query.encode("utf8")).decode().strip()


def bracket(value):
    return "[" + value.replace("]", "]]") + "]"


def stage_table(schema, table):
    # The source catalog is authoritative. Quoted literals and identifiers are separate.
    name = schema + "." + table
    literal = "'" + name.replace("'", "''") + "'"
    relation = bracket(schema) + "." + bracket(table)
    if target_sql(f"SELECT count(*) FROM source_stage.manifest WHERE name={literal}") == "1":
        return
    expected = int(source_sql(f"SELECT COUNT_BIG(*) FROM {relation}"))
    fields = source_sql(f"""SELECT c.name + '|' + TYPE_NAME(c.system_type_id)
      FROM sys.columns c WHERE object_id=OBJECT_ID({literal}) ORDER BY c.column_id""")
    projections = []
    for field in fields.splitlines():
        column, kind = field.strip().rsplit("|", 1)
        col = bracket(column)
        if kind in ("image", "timestamp", "rowversion"):
            projections.append(f"CONVERT(varbinary(max),t.{col}) AS {col}")
        else:
            projections.append(f"t.{col} AS {col}")
    query = f"SELECT (SELECT {','.join(projections)} FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) FROM {relation} t"
    run(["docker", "exec", SQL, "sh", "-ec",
         'export LANG=C.UTF-8; exec /opt/mssql-tools18/bin/bcp "$1" queryout /tmp/najaf-stage.ndjson -c -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d NajafRehearsal -u',
         "sh", query])
    rows = run(["docker", "exec", SQL, "cat", "/tmp/najaf-stage.ndjson"])
    # CSV control delimiters cannot occur literally inside valid JSON text.
    if rows.count(b"\n") != expected or b"\x01" in rows or b"\x02" in rows:
        raise ValueError(f"Record boundaries differ for {name}")
    command = f"""BEGIN;
CREATE TEMP TABLE raw_line(payload jsonb NOT NULL) ON COMMIT DROP;
COPY raw_line FROM STDIN WITH (FORMAT csv, DELIMITER E'\\x01', QUOTE E'\\x02');
""".encode()
    checksum = hashlib.sha256(rows).hexdigest()
    finish = f"""\\.
INSERT INTO source_stage.rows SELECT {literal},payload FROM raw_line;
DO $$ BEGIN IF (SELECT count(*) FROM raw_line) <> {expected} THEN RAISE EXCEPTION 'count mismatch'; END IF; END $$;
INSERT INTO source_stage.manifest VALUES ({literal},{expected},'{checksum}');
COMMIT;
""".encode()
    run(["docker", "exec", "-i", PG, "psql", "-Xq", "-U", "postgres", "-v", "ON_ERROR_STOP=1"],
        command + rows + finish)
    print(name, expected, flush=True)


def main():
    if source_sql("SELECT DATABASEPROPERTYEX(DB_NAME(),'Updateability')") != "READ_ONLY":
        raise ValueError("Source must be the read-only rehearsal")
    existing = run(["docker", "ps", "-a", "--filter", "name=^/"+PG+"$", "--format", "{{.Names}}"]).decode().strip()
    if not existing:
        env = dict(os.environ, POSTGRES_PASSWORD=secrets.token_urlsafe(40))
        subprocess.run(["docker", "run", "-d", "--network", "none", "--name", PG,
                        "-e", "POSTGRES_PASSWORD", "postgres:16-alpine"], env=env, check=True)
    run(["docker", "exec", PG, "sh", "-ec", "for i in $(seq 1 60); do pg_isready -U postgres && exit 0; sleep 1; done; exit 1"])
    target_sql("""CREATE SCHEMA IF NOT EXISTS source_stage;
    CREATE TABLE IF NOT EXISTS source_stage.rows(source_table text NOT NULL, payload jsonb NOT NULL);
    CREATE TABLE IF NOT EXISTS source_stage.manifest(name text PRIMARY KEY, expected bigint NOT NULL, sha256 text NOT NULL);""")
    catalog = source_sql("SELECT SCHEMA_NAME(schema_id)+'|'+name FROM sys.tables ORDER BY 1")
    for line in catalog.splitlines():
        stage_table(*line.strip().split("|", 1))
    print(target_sql("SELECT count(*),sum(expected) FROM source_stage.manifest;"))


if __name__ == "__main__":
    main()
