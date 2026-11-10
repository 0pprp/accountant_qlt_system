from pathlib import Path
import csv
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment
from docx import Document

base = Path(r"C:\GitHub\accountant_qlt_system\docs\sales-request-review")
csv_path = base / "Acceptance-04F.csv"
xlsx_path = base / "Acceptance-04F.xlsx"
docx_path = base / "Acceptance-04F.docx"

wb = Workbook()
ws = wb.active
ws.title = "Acceptance-04F"
header_fill = PatternFill("solid", fgColor="1F4E79")
header_font = Font(color="FFFFFF", bold=True)
rows = []
with csv_path.open(encoding="utf-8") as f:
    reader = csv.reader(f)
    for i, row in enumerate(reader):
        rows.append(row)
        ws.append(row)
        if i == 0:
            for cell in ws[1]:
                cell.fill = header_fill
                cell.font = header_font
                cell.alignment = Alignment(wrap_text=True)
for col in ws.columns:
    maxlen = max(len(str(c.value or "")) for c in col)
    ws.column_dimensions[col[0].column_letter].width = min(48, max(12, maxlen + 2))

ws2 = wb.create_sheet("Latency")
ws2.append(["Probe", "Origin", "Transport", "LatencyMs", "Claim_le_2s"])
ws2.append(["soft-delete Invalidation", "http://127.0.0.1:5173", "WebSockets", 336, True])
ws2.append(["restore Invalidation", "http://127.0.0.1:5173", "WebSockets", 1466, True])
ws2.append(["Note", "Panel Vue showed disconnected+polling in embedded browser", "", "", ""])

ws3 = wb.create_sheet("Evidence")
ws3.append(["Path", "Role"])
for p in [
    "docs/sales-request-review/Acceptance-04F.md",
    "docs/sales-request-review/04f-live-evidence/api-live-log.jsonl",
    "docs/sales-request-review/04f-live-evidence/api-live-log2.jsonl",
    "docs/sales-request-review/04f-live-evidence/api-live-log3.jsonl",
    "docs/sales-request-review/04f-live-evidence/signalr_latency.json",
    "docs/sales-request-review/04f-live-evidence/screenshots/",
]:
    ws3.append([p, "04F isolated evidence"])
wb.save(xlsx_path)

doc = Document()
doc.add_heading("Acceptance-04F — إغلاق دورة المبيعات والتحقق التشغيلي", level=1)
doc.add_paragraph(
    "لا يُعلن اكتمال كل شيء. الحالات: Implemented / Unit-tested / Integration-tested / Live-verified / Blocked / Not implemented."
)
doc.add_paragraph(
    "قاعدة الإثبات المعزولة: qalaat_04f_sales_verify على localhost/::1:5432. لا migration إنتاج. إنشاء الطلب عبر SQL موثّق وليس ingestion."
)
doc.add_heading("قياسات Realtime", level=2)
doc.add_paragraph("Origin http://127.0.0.1:5173 → hub /hubs/sales-requests عبر WebSockets.")
doc.add_paragraph("Latency save→event: 336 ms ثم 1466 ms (مقيسة؛ لا ادّعاء ≤2s بدون دليل).")
doc.add_paragraph("لوحة Vue في المتصفح المضمّن: غير متصل (polling محدود) — Live-blocked لاتصال UI.")
doc.add_heading("مصفوفة البنود", level=2)
table = doc.add_table(rows=1, cols=5)
hdr = table.rows[0].cells
hdr[0].text = "Area"
hdr[1].text = "Item"
hdr[2].text = "Live-verified"
hdr[3].text = "Blocked"
hdr[4].text = "Notes"
for row in rows[1:]:
    cells = table.add_row().cells
    cells[0].text = row[0]
    cells[1].text = row[1]
    cells[2].text = row[5]
    cells[3].text = row[6]
    cells[4].text = row[8] if len(row) > 8 else ""
doc.add_heading("Go / No-Go", level=2)
doc.add_paragraph("Go للمسار API+PostgreSQL المعزول (A/B، ensure-draft، complete، exceptions، delete، outbox recovery).")
doc.add_paragraph("No-Go لإعلان اكتمال الواجهة الحية realtime داخل اللوحة، ودورة Flutter على جهاز، وingestion/legal.")
doc.add_paragraph("التفاصيل الكاملة في Acceptance-04F.md")
doc.save(docx_path)
print("wrote", xlsx_path)
print("wrote", docx_path)
