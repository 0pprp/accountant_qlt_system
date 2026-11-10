from pathlib import Path
from datetime import datetime, timezone

import openpyxl
from docx import Document

root = Path(r"C:\GitHub\accountant_qlt_system\docs\parity-recovery")
xlsx = root / "Parity_Matrix.xlsx"
docx_path = root / "Architecture.docx"
progress = root / "Progress.md"

# --- Progress.md ---
text = progress.read_text(encoding="utf-8")
marker = "## فجوات متبقية"
safety_block = """## سلامة التحصيل والمزامنة (قبل 07 — harden 06)

عقد موحّد عبر `AppInstallmentPaymentWriter` لـ Create و Sync:

1. **ClientPaymentId** مربوط بـ CreatedBy + OrderId + Amount + Date.
   - نفس المفتاح ونفس البصمة → `alreadyApplied=true` (إقرار دون قبض ثانٍ).
   - نفس المفتاح وpayload مختلف → `409 ClientPaymentPayloadConflict`.
   - حساب آخر → `OrderNotFound` بلا كشف `paymentId` (لا Any(ClientPaymentId) وحده).
2. إعادة إقرار بعد `Completed` مسموحة بعد مطابقة الهوية؛ دفعة جديدة على مكتمل مرفوضة.
3. Flutter: لا إعادة كتابة OrderId تلقائياً للمعلّق؛ عند غموض/فقدان وصول تُحفظ العملية مع سبب التعثر؛ الحذف المحلي فقط بعد إقرار يثبت نفس المفتاح/الطلب/المبلغ.
4. 403/400/نص خطأ ≠ نجاح مكرر. Create→Sync يستخدم نفس تاريخ التحصيل.
5. قفل ذري `pg_advisory_xact_lock` على الطلب وحامل النقد داخل معاملة واحدة.

### API-live safety (`evidence/06-delegate/api-live-payment-safety.log`)
```
CONCURRENT Create+Sync KEY_COUNT=1
CONFLICT_AMOUNT_STATUS=409
OTHER_ACCOUNT_STATUS=400 leakPaymentId=False
ACK_AFTER_COMPLETE already=True؛ NEW_AFTER_COMPLETE=400
OVER_REMAINING okCount=1 errCount=1
TWO_CONCURRENT_CASH delta=2000 ok=True
RETRY_CREATE_THEN_SYNC ack=True sameId=True orderId=1
```

### Unit safety
`ClientPaymentIdSafetyTests` — Passed (fingerprint / 409 / other-account / completed-ack / remaining).

"""

if "سلامة التحصيل والمزامنة" not in text:
    text = text.replace(marker, safety_block + marker)
else:
    # replace existing safety section through OpenGaps
    import re
    text = re.sub(
        r"## سلامة التحصيل والمزامنة.*?(?=## فجوات متبقية)",
        safety_block,
        text,
        flags=re.S,
    )

text = text.replace(
    "تاريخ التحديث: 2026-11-10 (دفعات 05–06)",
    "تاريخ التحديث: 2026-11-10 (دفعات 05–06 + سلامة التحصيل)",
)
# bump 06 proof line
if "api-live-payment-safety.log" not in text.split("## أوامر مثبتة")[-1]:
    text = text.replace(
        "  evidence/06-delegate/api-live-06.log\n```",
        "  evidence/06-delegate/api-live-06.log\n"
        "  evidence/06-delegate/api-live-payment-safety.log\n"
        "dotnet test --filter ClientPaymentIdSafety → Passed\n```",
    )
# status row for 06
text = text.replace(
    "| 06 | مندوب جباية + مزامنة + تقييم أول | Implemented | Unit+API-live؛ Device معلّق (لا adb) |",
    "| 06 | مندوب جباية + مزامنة + تقييم أول | Implemented + payment-safety | Unit+API-live (idempotency/locks)؛ Device معلّق |",
)
progress.write_text(text, encoding="utf-8")

# --- Excel ---
wb = openpyxl.load_workbook(xlsx)
ws = wb["05_06_Updates"]
# append safety row if missing
existing = [str(r[1].value or "") for r in ws.iter_rows(min_row=2)]
if not any("payment-safety" in e or "AppInstallmentPaymentWriter" in e for e in existing):
    ws.append(
        [
            "06-safety",
            "Collect/sync ClientPaymentId ownership+atomic cash",
            "CustomersPayments fingerprint",
            "AppInstallmentPaymentWriter Create+Sync; alreadyApplied; 409 conflict",
            "Y",
            "Y",
            "Y",
            "N",
            "Partial",
            "dual-conn PG: same key, mismatch 409, other acct no leak, completed ack, over-remaining, concurrent cash delta=2000",
        ]
    )
wb.save(xlsx)

# --- Word ---
doc = Document(docx_path)
doc.add_heading("Payment safety harden (pre-07)", level=1)
doc.add_paragraph(
    f"Updated {datetime.now(timezone.utc).isoformat()} — iso DB only; no production; no batch 07."
)
doc.add_heading("Contract", level=2)
for line in [
    "Unified AppInstallmentPaymentWriter for Create and Sync",
    "ClientPaymentId bound to owner (CreatedBy) + OrderId + Amount + Date",
    "Same key+fingerprint → alreadyApplied ack; mismatch → 409; other account → OrderNotFound without payment leak",
    "Completed-sale retry ack allowed; new payment on Completed rejected",
    "No automatic OrderId rewrite on pending queue; keep item + failure reason",
    "Local delete only after server ack proving same operation; 403/text ≠ duplicate success",
    "pg_advisory_xact_lock on order + cash holder inside one transaction",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("Live proof", level=2)
doc.add_paragraph(
    "evidence/06-delegate/api-live-payment-safety.log — concurrent Create/Sync KEY_COUNT=1; "
    "409 amount conflict; other account 400 leakPaymentId=False; completed ack; "
    "over-remaining 1ok/1err; concurrent cash delta=2000; Create→Sync retry sameId+orderId."
)
doc.save(docx_path)
print("UPDATED", progress, xlsx, docx_path)
