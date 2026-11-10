from pathlib import Path
from datetime import datetime, timezone

import openpyxl
from docx import Document

root = Path(r"C:\GitHub\accountant_qlt_system\docs\parity-recovery")
progress = root / "Progress.md"
text = progress.read_text(encoding="utf-8")
old = "5. قفل ذري `pg_advisory_xact_lock` على الطلب وحامل النقد داخل معاملة واحدة."
new = (
    "5. قفل ذري `pg_advisory_xact_lock(namespace, entityId)` — طلب=`791902` / نقد=`791903` "
    "(بعيد عن ensure-draft=`791901`؛ بلا GetHashCode). الترتيب: طلب ثم حامل نقد.\n"
    "6. Flutter `parseServerAck`: ClientPaymentId+OrderId+Amount+Date + PaymentId>0 قبل الحذف المحلي.\n"
    "7. إثبات عمليتي API (4200+4201): Create∥Sync عبر عمليتين؛ تجاوز متبقي؛ طلبان لنفس الحامل بلا فقد رصيد — "
    "`api-live-dual-process-locks.log`."
)
if old in text:
    text = text.replace(old, new)
if "api-live-dual-process-locks.log" not in text:
    text = text.replace(
        "  evidence/06-delegate/api-live-payment-safety.log\n",
        "  evidence/06-delegate/api-live-payment-safety.log\n"
        "  evidence/06-delegate/api-live-dual-process-locks.log\n",
    )
text = text.replace(
    "`ClientPaymentIdSafetyTests` — Passed (fingerprint / 409 / other-account / completed-ack / remaining).",
    "`ClientPaymentIdSafetyTests` — Passed 6 (incl. lock namespaces). Flutter `payment_ack_parse_test` — Passed.",
)
progress.write_text(text, encoding="utf-8")

wb = openpyxl.load_workbook(root / "Parity_Matrix.xlsx")
ws = wb["05_06_Updates"]
vals = [str(r[1].value or "") for r in ws.iter_rows(min_row=2)]
if not any("dual-process" in v or "791902" in v or "Advisory locks" in v for v in vals):
    ws.append(
        [
            "06-locks",
            "Advisory locks + Flutter ack Date/PaymentId",
            "pg_advisory_xact_lock two-arg",
            "791902 order / 791903 cash; parseServerAck Date+PaymentId",
            "Y",
            "Y",
            "Y",
            "N",
            "Partial",
            "dual API 4200/4201; CROSS_CREATE_SYNC; OVER_DUAL; DUAL_ORDERS_SAME_CASH delta=2000",
        ]
    )
wb.save(root / "Parity_Matrix.xlsx")

doc = Document(root / "Architecture.docx")
doc.add_heading("Advisory lock namespaces + Flutter ack harden", level=1)
doc.add_paragraph(
    f"Updated {datetime.now(timezone.utc).isoformat()} — iso only; no production."
)
for line in [
    "Removed GetHashCode lock keys",
    "pg_advisory_xact_lock(791902, orderId) then pg_advisory_xact_lock(791903, sellerId)",
    "Namespaces distinct from ensure-draft 791901 and single-key submission locks",
    "Flutter parseServerAck requires ClientPaymentId+OrderId+Amount+Date match and PaymentId>0 before local delete",
    "Live: api-live-dual-process-locks.log on two API processes :4200 and :4201",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.save(root / "Architecture.docx")
print("DOCS_OK")
