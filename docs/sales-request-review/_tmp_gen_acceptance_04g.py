from pathlib import Path
import csv
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment
from docx import Document

base = Path(r"C:\GitHub\accountant_qlt_system\docs\sales-request-review")
csv_path = base / "Acceptance-04G.csv"
xlsx_path = base / "Acceptance-04G.xlsx"
docx_path = base / "Acceptance-04G.docx"

wb = Workbook()
ws = wb.active
ws.title = "Acceptance-04G"
header_fill = PatternFill("solid", fgColor="0F3D3E")
header_font = Font(color="FFFFFF", bold=True)
rows = []
with csv_path.open(encoding="utf-8") as f:
    for i, row in enumerate(csv.reader(f)):
        rows.append(row)
        ws.append(row)
        if i == 0:
            for cell in ws[1]:
                cell.fill = header_fill
                cell.font = header_font
for col in ws.columns:
    maxlen = max(len(str(c.value or "")) for c in col)
    ws.column_dimensions[col[0].column_letter].width = min(52, max(12, maxlen + 2))

ws2 = wb.create_sheet("EvidenceSplit")
ws2.append(["Layer", "CountsAs"])
ws2.append(["API/app cycle", "Live-verified contracts — not Flutter UI"])
ws2.append(["Standalone SignalR (04F)", "Not Vue proof"])
ws2.append(["Vue salesRequestRealtime", "Live-verified Hub UI"])
ws2.append(["Flutter windows/web UI", "Live-blocked (VS toolchain)"])
wb.save(xlsx_path)

doc = Document()
doc.add_heading("Acceptance-04G — إغلاق ربط الواجهات الفعلية", level=1)
doc.add_paragraph("لا يُعلن اكتمال Flutter على جهاز ولا Legal/ingestion.")
doc.add_paragraph("Vue Hub: متصل عبر العميل الفعلي (Vite proxy + RequireCors). تحديث التفاصيل/العدادات بلا reload مثبت.")
doc.add_paragraph("Flutter UI: Live-blocked — Unable to find suitable Visual Studio toolchain. دورة app/* موثّقة منفصلة.")
table = doc.add_table(rows=1, cols=4)
hdr = table.rows[0].cells
hdr[0].text = "Area"
hdr[1].text = "Item"
hdr[2].text = "Live-verified"
hdr[3].text = "Blocked"
for row in rows[1:]:
    cells = table.add_row().cells
    cells[0].text = row[0]
    cells[1].text = row[1]
    cells[2].text = row[5]
    cells[3].text = row[6]
doc.save(docx_path)
print("wrote", xlsx_path)
print("wrote", docx_path)
