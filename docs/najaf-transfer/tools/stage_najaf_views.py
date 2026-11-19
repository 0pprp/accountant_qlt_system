"""Stage financial views needed for match (tables alone lack AmountDenar)."""
import hashlib
import subprocess

SQL = "najaf-rehearsal-bluawosx"
PG = "najaf-pg-bluawosx"
VIEWS = [
    ("dbo.View_CustomersRemaining", "CustomerID,DelegateID,AmountRemaining"),
    (
        "dbo.View_CustomersSales",
        "CustomerSaleID,CustomerID,AmountTotalSalesDenar,ReceiptsTotal,AmountRemaining,DateCreate",
    ),
    (
        "dbo.View_AddToBox",
        "AddToBoxID,CustomerPaymentID,CustomerID,CustomerIDPayment,Amount,AmountDenar,DateCreate,BoxID",
    ),
]


def run(args, data=None):
    return subprocess.run(args, input=data, capture_output=True, check=True).stdout


def source_sql(query):
    return run(
        [
            "docker",
            "exec",
            SQL,
            "sh",
            "-ec",
            'export SQLCMDPASSWORD="$MSSQL_SA_PASSWORD"; exec /opt/mssql-tools18/bin/sqlcmd -C -I -U sa -S localhost -b -h -1 -W -d NajafRehearsal -Q "$1"',
            "sh",
            "SET NOCOUNT ON; " + query,
        ]
    ).decode("utf-8-sig").strip()


def target_sql(query):
    return run(
        ["docker", "exec", "-i", PG, "psql", "-XqAt", "-v", "ON_ERROR_STOP=1", "-U", "postgres", "-d", "postgres"],
        query.encode("utf8"),
    ).decode().strip()


def stage_view(name: str, columns: str) -> None:
    literal = "'" + name.replace("'", "''") + "'"
    if target_sql(f"SELECT count(*) FROM source_stage.manifest WHERE name={literal}") == "1":
        print(name, "skip", flush=True)
        return
    expected = int(source_sql(f"SELECT COUNT_BIG(*) FROM {name}"))
    # -q enables QUOTED_IDENTIFIER required by indexed/computed views
    query = (
        "SELECT (SELECT "
        + ",".join(f"t.[{c.strip()}] AS [{c.strip()}]" for c in columns.split(","))
        + " FOR JSON PATH, INCLUDE_NULL_VALUES, WITHOUT_ARRAY_WRAPPER) FROM "
        + name
        + " t"
    )
    proc = subprocess.run(
        [
            "docker",
            "exec",
            SQL,
            "sh",
            "-ec",
            'export LANG=C.UTF-8; exec /opt/mssql-tools18/bin/bcp "$1" queryout /tmp/najaf-stage.ndjson -c -q -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -d NajafRehearsal -u',
            "sh",
            query,
        ],
        capture_output=True,
    )
    if proc.returncode != 0:
        raise RuntimeError(proc.stderr.decode("utf-8", "replace") or proc.stdout.decode("utf-8", "replace"))
    rows = run(["docker", "exec", SQL, "cat", "/tmp/najaf-stage.ndjson"])
    line_count = rows.count(b"\n")
    if line_count != expected:
        raise ValueError(f"count mismatch {name}: lines={line_count} expected={expected}")
    checksum = hashlib.sha256(rows).hexdigest()
    command = f"""BEGIN;
CREATE TEMP TABLE raw_line(payload jsonb NOT NULL) ON COMMIT DROP;
COPY raw_line FROM STDIN WITH (FORMAT csv, DELIMITER E'\\x01', QUOTE E'\\x02');
""".encode()
    finish = f"""\\.
INSERT INTO source_stage.rows SELECT {literal},payload FROM raw_line;
DO $$ BEGIN IF (SELECT count(*) FROM raw_line) <> {expected} THEN RAISE EXCEPTION 'count mismatch'; END IF; END $$;
INSERT INTO source_stage.manifest VALUES ({literal},{expected},'{checksum}');
COMMIT;
""".encode()
    run(
        ["docker", "exec", "-i", PG, "psql", "-Xq", "-U", "postgres", "-v", "ON_ERROR_STOP=1"],
        command + rows + finish,
    )
    print(name, expected, checksum[:12], flush=True)


def main():
    target_sql(
        """CREATE SCHEMA IF NOT EXISTS source_stage;
    CREATE TABLE IF NOT EXISTS source_stage.rows(source_table text NOT NULL, payload jsonb NOT NULL);
    CREATE TABLE IF NOT EXISTS source_stage.manifest(name text PRIMARY KEY, expected bigint NOT NULL, sha256 text NOT NULL);"""
    )
    for name, cols in VIEWS:
        stage_view(name, cols)
    print(target_sql("SELECT count(*),sum(expected) FROM source_stage.manifest;"))


if __name__ == "__main__":
    main()
