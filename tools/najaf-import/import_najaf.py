#!/usr/bin/env python3
"""Idempotent Najaf branch importer: source_stage -> sandbox Postgres.

Does not touch user 'ahmed' or Admin permissions.
ImportKey format: najaf|{sourceTable}|{sourceId}
Historical import only: no stock deduction, cash posts, or notifications.
"""
from __future__ import annotations

import argparse
import json
import os
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

try:
    import psycopg
except ImportError:  # pragma: no cover
    psycopg = None  # type: ignore

STATE_DIR = Path(os.environ.get("NAJAF_IMPORT_STATE_DIR", Path(__file__).resolve().parent / ".local"))
PROTECTED_USERNAMES = {"ahmed", "admin", "administrator"}


def now_baghdad_iso() -> str:
    return datetime.now(timezone.utc).astimezone().strftime("%Y-%m-%dT%H:%M:%S+03:00")


def load_state(path: Path) -> dict[str, Any]:
    if path.exists():
        return json.loads(path.read_text(encoding="utf-8"))
    return {
        "snapshot_id": None,
        "branch_name": None,
        "last_step": None,
        "updated_at_baghdad": None,
        "counts": {},
        "exceptions": 0,
    }


def save_state(path: Path, state: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(state, ensure_ascii=False, indent=2), encoding="utf-8")


def append_exception(path: Path, row: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("a", encoding="utf-8") as f:
        f.write(json.dumps(row, ensure_ascii=False) + "\n")


def import_key(table: str, source_id: Any) -> str:
    return f"najaf|{table}|{source_id}"


def require_psycopg() -> None:
    if psycopg is None:
        raise SystemExit("psycopg is required: pip install 'psycopg[binary]'")


def connect(dsn: str):
    require_psycopg()
    return psycopg.connect(dsn)


def ensure_import_map(cur) -> None:
    cur.execute(
        """
        CREATE SCHEMA IF NOT EXISTS najaf_import;
        CREATE TABLE IF NOT EXISTS najaf_import.map (
          import_key text PRIMARY KEY,
          entity_type text NOT NULL,
          entity_id bigint NOT NULL,
          source_table text NOT NULL,
          created_at timestamptz NOT NULL DEFAULT now()
        );
        CREATE TABLE IF NOT EXISTS najaf_import.exceptions (
          id bigserial PRIMARY KEY,
          import_key text,
          step text NOT NULL,
          detail jsonb NOT NULL,
          created_at timestamptz NOT NULL DEFAULT now()
        );
        """
    )


def resolve_branch_id(cur, branch_name: str, create_if_missing: bool) -> int:
    cur.execute('SELECT "Id" FROM "Branches" WHERE "Name" = %s LIMIT 1', (branch_name,))
    row = cur.fetchone()
    if row:
        return int(row[0])
    if not create_if_missing:
        raise RuntimeError(f"Branch not found: {branch_name}")
    cur.execute(
        """
        INSERT INTO "Branches" ("Name", "Created", "CreatedBy")
        VALUES (%s, now(), 'najaf-import')
        RETURNING "Id"
        """,
        (branch_name,),
    )
    return int(cur.fetchone()[0])


def stage_rows(cur, table: str):
    name = table if "." in table else f"dbo.{table}"
    cur.execute(
        "SELECT payload FROM source_stage.rows WHERE source_table = %s",
        (name,),
    )
    for (payload,) in cur:
        yield payload if isinstance(payload, dict) else json.loads(payload)


def mapped_id(cur, key: str) -> int | None:
    cur.execute("SELECT entity_id FROM najaf_import.map WHERE import_key = %s", (key,))
    row = cur.fetchone()
    return int(row[0]) if row else None


def remember(cur, key: str, entity_type: str, entity_id: int, source_table: str) -> None:
    cur.execute(
        """
        INSERT INTO najaf_import.map (import_key, entity_type, entity_id, source_table)
        VALUES (%s, %s, %s, %s)
        ON CONFLICT (import_key) DO NOTHING
        """,
        (key, entity_type, entity_id, source_table),
    )


def parse_date(value: Any):
    if value is None:
        return None
    s = str(value)
    if "T" in s:
        return s.split("T", 1)[0]
    return s[:10]


def step_guard_ahmed(target) -> None:
    with target.cursor() as cur:
        cur.execute(
            """
            SELECT "UserName" FROM "AspNetUsers"
            WHERE lower("UserName") = ANY(%s)
            """,
            (list(PROTECTED_USERNAMES),),
        )
        found = [r[0] for r in cur.fetchall()]
        if found and "ahmed" not in {u.lower() for u in found}:
            raise RuntimeError("Protected user ahmed missing — abort")


def step_customers(stage, target, branch_id: int, dry_run: bool, exc_path: Path, counts: dict) -> None:
    inserted = skipped = errors = 0
    with stage.cursor() as sc, target.cursor() as tc:
        ensure_import_map(tc)
        for payload in stage_rows(sc, "dbo.Customers"):
            src_id = payload.get("CustomerID")
            key = import_key("Customers", src_id)
            if mapped_id(tc, key) is not None:
                skipped += 1
                continue
            if dry_run:
                inserted += 1
                continue
            try:
                full_name = (payload.get("CustomerName") or f"Najaf-{src_id}")[:100]
                phone = str(payload.get("PhoneNumber") or "")[:30]
                tc.execute(
                    """
                    INSERT INTO "Customers" (
                      "FullName", "MotherName", "NationalCode", "BirthDate",
                      "PhoneNumber", "WhatsAppPhoneNumber", "BranchId",
                      "IsLegal", "IsFakeSale", "Created", "CreatedBy", "ImportKey"
                    ) VALUES (
                      %s, '', '', '1970-01-01',
                      %s, %s, %s,
                      COALESCE(%s,false), COALESCE(%s,false), now(), 'najaf-import', %s
                    )
                    RETURNING "Id"
                    """,
                    (
                        full_name,
                        phone,
                        phone,
                        branch_id,
                        payload.get("IsLegal"),
                        payload.get("IsFakeSale"),
                        key,
                    ),
                )
                row = tc.fetchone()
                remember(tc, key, "Customer", int(row[0]), "Customers")
                inserted += 1
            except Exception as ex:  # noqa: BLE001
                errors += 1
                target.rollback()
                ensure_import_map(tc)
                detail = {"error": str(ex), "source_id": src_id}
                append_exception(exc_path, {"step": "customers", "import_key": key, "detail": detail})
                try:
                    tc.execute(
                        "INSERT INTO najaf_import.exceptions (import_key, step, detail) VALUES (%s,%s,%s::jsonb)",
                        (key, "customers", json.dumps(detail, ensure_ascii=False)),
                    )
                    target.commit()
                except Exception:  # noqa: BLE001
                    target.rollback()
        if not dry_run:
            target.commit()
    counts["customers_inserted"] = inserted
    counts["customers_skipped"] = skipped
    counts["customers_errors"] = errors


def step_orders(stage, target, branch_id: int, dry_run: bool, exc_path: Path, counts: dict) -> None:
    """Prefer View_CustomersSales (has AmountTotalSalesDenar); fallback CustomersSales ids only."""
    inserted = skipped = errors = 0
    source_table = "dbo.View_CustomersSales"
    with stage.cursor() as sc:
        sc.execute("SELECT 1 FROM source_stage.manifest WHERE name=%s", (source_table,))
        if not sc.fetchone():
            source_table = "dbo.CustomersSales"

    with stage.cursor() as sc, target.cursor() as tc:
        for payload in stage_rows(sc, source_table):
            src_id = payload.get("CustomerSaleID")
            key = import_key("CustomersSales", src_id)
            if mapped_id(tc, key) is not None:
                skipped += 1
                continue
            cust_key = import_key("Customers", payload.get("CustomerID"))
            cust_id = mapped_id(tc, cust_key)
            if cust_id is None and not dry_run:
                errors += 1
                detail = {"error": "customer_missing", "customer_id": payload.get("CustomerID")}
                append_exception(exc_path, {"step": "orders", "import_key": key, "detail": detail})
                continue
            if dry_run:
                inserted += 1
                continue
            try:
                sell = float(payload.get("AmountTotalSalesDenar") or payload.get("Amount") or 0)
                prep = float(payload.get("FirstAmountDenar") or payload.get("FirstAmount") or 0)
                tc.execute(
                    """
                    INSERT INTO "Orders" (
                      "BuyAmount", "SellAmount", "PrepaymentAmount", "DailyInstallmentAmount",
                      "CustomerId", "BranchId", "ImportKey", "Created", "CreatedBy"
                    ) VALUES (%s,%s,%s,0,%s,%s,%s,now(),'najaf-import')
                    RETURNING "Id"
                    """,
                    (sell, sell, prep, cust_id, branch_id, key),
                )
                row = tc.fetchone()
                remember(tc, key, "Order", int(row[0]), "CustomersSales")
                inserted += 1
            except Exception as ex:  # noqa: BLE001
                errors += 1
                target.rollback()
                detail = {"error": str(ex), "source_id": src_id}
                append_exception(exc_path, {"step": "orders", "import_key": key, "detail": detail})
        if not dry_run:
            target.commit()
    counts["orders_source"] = source_table
    counts["orders_inserted"] = inserted
    counts["orders_skipped"] = skipped
    counts["orders_errors"] = errors


def step_payments(stage, target, dry_run: bool, exc_path: Path, counts: dict) -> None:
    """Customer-level receipts from View_AddToBox (AmountDenar + CustomerIDPayment)."""
    inserted = skipped = errors = unallocated = 0
    source_table = "dbo.View_AddToBox"
    with stage.cursor() as sc:
        sc.execute("SELECT 1 FROM source_stage.manifest WHERE name=%s", (source_table,))
        if not sc.fetchone():
            source_table = "dbo.AddToBox"

    with stage.cursor() as sc, target.cursor() as tc:
        for payload in stage_rows(sc, source_table):
            # Prefer payment id; else AddToBox id
            src_id = payload.get("CustomerPaymentID") or payload.get("AddToBoxID")
            if src_id is None:
                continue
            cust_src = payload.get("CustomerIDPayment") or payload.get("CustomerID")
            if cust_src is None:
                continue
            amount = payload.get("AmountDenar")
            if amount is None:
                amount = payload.get("Amount")
            if amount is None:
                continue
            key = import_key("CustomersPayments", src_id)
            if mapped_id(tc, key) is not None:
                skipped += 1
                continue
            if dry_run:
                inserted += 1
                continue
            cust_id = mapped_id(tc, import_key("Customers", cust_src))
            if cust_id is None:
                errors += 1
                continue
            try:
                pay_date = parse_date(payload.get("DateCreate") or payload.get("PaymentDate")) or "1970-01-01"
                tc.execute(
                    """
                    INSERT INTO "InstallmentPayments" (
                      "Amount", "Date", "Description", "ImportKey", "OrderId", "CustomerId",
                      "Created", "CreatedBy"
                    ) VALUES (%s, %s::date, %s, %s, NULL, %s, now(), 'najaf-import')
                    RETURNING "Id"
                    """,
                    (float(amount), pay_date, "najaf-unallocated", key, cust_id),
                )
                row = tc.fetchone()
                remember(tc, key, "InstallmentPayment", int(row[0]), "CustomersPayments")
                inserted += 1
                unallocated += 1
            except Exception as ex:  # noqa: BLE001
                errors += 1
                target.rollback()
                detail = {"error": str(ex), "source_id": src_id}
                append_exception(exc_path, {"step": "payments", "import_key": key, "detail": detail})
        if not dry_run:
            target.commit()
    counts["payments_source"] = source_table
    counts["payments_inserted"] = inserted
    counts["payments_skipped"] = skipped
    counts["payments_errors"] = errors
    counts["payments_unallocated"] = unallocated


def run_manifest_check(stage) -> dict[str, Any]:
    with stage.cursor() as cur:
        cur.execute("SELECT count(*), coalesce(sum(expected),0) FROM source_stage.manifest")
        tables, rows = cur.fetchone()
        cur.execute("SELECT name, expected, sha256 FROM source_stage.manifest ORDER BY name")
        manifest = [{"name": n, "expected": int(e), "sha256": s} for n, e, s in cur.fetchall()]
    return {"tables": int(tables), "rows": int(rows), "manifest": manifest}


def main() -> int:
    parser = argparse.ArgumentParser(description="Najaf sandbox importer")
    parser.add_argument("--dry-run", action="store_true")
    parser.add_argument("--branch-name", default=os.environ.get("NAJAF_BRANCH_NAME", "النجف"))
    parser.add_argument("--snapshot-id", default=os.environ.get("NAJAF_SNAPSHOT_ID", "3dfc133f"))
    args = parser.parse_args()

    stage_dsn = os.environ.get("NAJAF_STAGE_DSN")
    target_dsn = os.environ.get("NAJAF_TARGET_DSN")
    if not stage_dsn or not target_dsn:
        print("Set NAJAF_STAGE_DSN and NAJAF_TARGET_DSN", file=sys.stderr)
        return 2

    state_path = STATE_DIR / "state.json"
    exc_path = STATE_DIR / "exceptions.jsonl"
    state = load_state(state_path)
    state["snapshot_id"] = args.snapshot_id
    state["branch_name"] = args.branch_name
    state["updated_at_baghdad"] = now_baghdad_iso()

    with connect(stage_dsn) as stage, connect(target_dsn) as target:
        state["last_step"] = "manifest"
        manifest_info = run_manifest_check(stage)
        state["counts"]["stage_tables"] = manifest_info["tables"]
        state["counts"]["stage_rows"] = manifest_info["rows"]
        save_state(state_path, state)

        state["last_step"] = "guard_ahmed"
        step_guard_ahmed(target)

        with target.cursor() as cur:
            ensure_import_map(cur)
            branch_id = resolve_branch_id(cur, args.branch_name, create_if_missing=not args.dry_run)
            if not args.dry_run:
                target.commit()
        state["counts"]["branch_id"] = None if args.dry_run else branch_id

        state["last_step"] = "customers"
        step_customers(stage, target, 0 if args.dry_run else branch_id, args.dry_run, exc_path, state["counts"])
        state["last_step"] = "orders"
        step_orders(stage, target, 0 if args.dry_run else branch_id, args.dry_run, exc_path, state["counts"])
        state["last_step"] = "payments"
        step_payments(stage, target, args.dry_run, exc_path, state["counts"])

        state["exceptions"] = (
            state["counts"].get("customers_errors", 0)
            + state["counts"].get("orders_errors", 0)
            + state["counts"].get("payments_errors", 0)
        )
        state["updated_at_baghdad"] = now_baghdad_iso()
        state["last_step"] = "done_dry_run" if args.dry_run else "done"
        save_state(state_path, state)

        STATE_DIR.mkdir(parents=True, exist_ok=True)
        (STATE_DIR / "manifest_summary.json").write_text(
            json.dumps(
                {
                    "tables": manifest_info["tables"],
                    "rows": manifest_info["rows"],
                    "sha_sample": [m["sha256"][:12] for m in manifest_info["manifest"][:5]],
                },
                indent=2,
            ),
            encoding="utf-8",
        )

    print(json.dumps(state, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
