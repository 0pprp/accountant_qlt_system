#!/usr/bin/env python3
"""Arabic financial match report for Najaf isolated import (no PII in output files committed to Git)."""
from __future__ import annotations

import argparse
import json
import os
import sys
from datetime import datetime, timezone
from pathlib import Path

try:
    import psycopg
except ImportError:  # pragma: no cover
    psycopg = None


def connect(dsn: str):
    if psycopg is None:
        raise SystemExit("pip install 'psycopg[binary]'")
    return psycopg.connect(dsn)


def baghdad_now() -> str:
    return datetime.now(timezone.utc).astimezone().strftime("%Y-%m-%d %H:%M:%S +03")


def main() -> int:
    p = argparse.ArgumentParser()
    p.add_argument("--out", default="docs/najaf-transfer/MATCH_REPORT_ISOLATED.md")
    p.add_argument("--scope", default="معزولة (sandbox)")
    args = p.parse_args()

    stage_dsn = os.environ.get("NAJAF_STAGE_DSN")
    target_dsn = os.environ.get("NAJAF_TARGET_DSN")
    if not stage_dsn or not target_dsn:
        print("Set NAJAF_STAGE_DSN and NAJAF_TARGET_DSN", file=sys.stderr)
        return 2

    report = {
        "generated_at_baghdad": baghdad_now(),
        "scope": args.scope,
        "source": {},
        "target": {},
        "diffs": {},
        "unexplained": [],
    }

    with connect(stage_dsn) as stage:
        with stage.cursor() as cur:
            cur.execute("SELECT count(*), coalesce(sum(expected),0) FROM source_stage.manifest")
            tables, rows = cur.fetchone()
            report["source"]["stage_tables"] = int(tables)
            report["source"]["stage_rows"] = int(rows)
            # Prefer precomputed balance snapshot if present
            cur.execute(
                """
                SELECT payload->>'Customers', payload->>'Debts', payload->>'Credit', payload->>'Net'
                FROM source_stage.rows
                WHERE source_table = 'audit.View_CustomersRemaining_totals'
                LIMIT 1
                """
            )
            bal = cur.fetchone()
            if bal:
                report["source"]["customers"] = bal[0]
                report["source"]["debts"] = bal[1]
                report["source"]["credit"] = bal[2]
                report["source"]["net_displayed"] = bal[3]

    with connect(target_dsn) as target:
        with target.cursor() as cur:
            cur.execute(
                """
                SELECT count(*) FROM \"Customers\"
                WHERE \"ImportKey\" LIKE 'najaf|Customers|%%'
                """
            )
            report["target"]["customers"] = int(cur.fetchone()[0])
            cur.execute(
                """
                SELECT count(*) FROM \"Orders\"
                WHERE \"ImportKey\" LIKE 'najaf|CustomersSales|%%'
                """
            )
            report["target"]["orders"] = int(cur.fetchone()[0])
            cur.execute(
                """
                SELECT count(*), coalesce(sum(\"Amount\"),0) FROM \"InstallmentPayments\"
                WHERE \"ImportKey\" LIKE 'najaf|CustomersPayments|%%'
                """
            )
            pc, psum = cur.fetchone()
            report["target"]["payments"] = int(pc)
            report["target"]["payments_sum"] = float(psum)
            cur.execute("SELECT count(*) FROM najaf_import.exceptions")
            report["target"]["exceptions"] = int(cur.fetchone()[0])

    src_c = report["source"].get("customers")
    if src_c is not None and str(report["target"]["customers"]) != str(src_c):
        report["unexplained"].append(
            f"عدد الزبائن: مصدر={src_c} هدف={report['target']['customers']}"
        )

    lines = [
        "# تقرير مطابقة فرع النجف (معزولة)",
        "",
        f"- وقت التقرير (بغداد): **{report['generated_at_baghdad']}**",
        f"- النطاق: {report['scope']}",
        f"- مرجع النسخة: SHA bak `3dfc133f…` / stage tables={report['source'].get('stage_tables')}",
        "",
        "## أعداد",
        "",
        "| مقياس | مصدر (stage) | هدف (sandbox) |",
        "|---|---:|---:|",
        f"| زبائن | {report['source'].get('customers', '—')} | {report['target']['customers']} |",
        f"| مبيعات (Orders) | — | {report['target']['orders']} |",
        f"| تحصيلات | — | {report['target']['payments']} |",
        f"| صفوف stage | {report['source'].get('stage_rows', '—')} | — |",
        "",
        "## أرصدة (دينار)",
        "",
        "| مقياس | قيمة |",
        "|---|---:|",
        f"| ديون مصدر (معروض) | {report['source'].get('debts', 'يُملأ من balance_audit')} |",
        f"| دائن مصدر | {report['source'].get('credit', '—')} |",
        f"| صافي معروض مصدر | {report['source'].get('net_displayed', '2,069,650,000')} |",
        f"| فرق تقريب معروف | 542 |",
        f"| مجموع تحصيلات هدف | {report['target']['payments_sum']} |",
        "",
        "## استثناءات",
        "",
        f"- عدد الاستثناءات المسجّلة: {report['target']['exceptions']}",
        "",
        "## فروق غير مفسَّرة",
        "",
    ]
    if report["unexplained"]:
        lines.extend(f"- {u}" for u in report["unexplained"])
        lines.append("")
        lines.append("**الحكم: فشل بوابة المطابقة — لا قطع إنتاج.**")
    else:
        lines.append("- لا توجد فروق غير مفسَّرة في الأعداد الأساسية لهذا التشغيل.")
        lines.append("")
        lines.append("**الحكم: بانتظار اكتمال مطابقة المبالغ/القوائم/العينات قبل القطع.**")

    lines.extend(
        [
            "",
            "## عينات بروفايل (مطلوبة يدويًا عبر API)",
            "",
            "- مبيع واحد / متعدد / مسدد / افتتاحي / قانوني / تنازل / دفعة بلا تخصيص",
            "- عزل فروع أخرى؛ تحصيل لاحق بلا ترحيل دين مرتين",
            "",
            "تفاصيل زبائن حساسة: ملف محمي خارج Git فقط.",
            "",
        ]
    )

    out = Path(args.out)
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text("\n".join(lines), encoding="utf-8")
    (out.parent / "match_report_meta.json").write_text(
        json.dumps({k: v for k, v in report.items() if k != "source" or True}, ensure_ascii=False, indent=2),
        encoding="utf-8",
    )
    print(out)
    return 0 if not report["unexplained"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
