import csv
from collections import defaultdict
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Font, Alignment, Border, Side, PatternFill
from openpyxl.utils import get_column_letter

import sys

date = sys.argv[1] if len(sys.argv) > 1 else "2026-11-17"
src = Path(rf"C:\GitHub\accountant_qlt_system\docs\najaf-payments-{date}.csv")
out = Path(rf"C:\GitHub\accountant_qlt_system\docs\تسديدات_النجف_{date}.xlsx")

rows_by_list: dict[str, list[dict]] = defaultdict(list)
with src.open(encoding="utf-8-sig", newline="") as f:
    for r in csv.DictReader(f):
        name = (r.get("list_name") or "بدون قائمة").strip() or "بدون قائمة"
        rows_by_list[name].append(r)

wb = Workbook()
ws0 = wb.active
ws0.title = "ملخص"
ws0.append(["القائمة", "المندوب", "عدد التسديدات", "المبلغ الكلي"])

thin = Border(
    left=Side(style="thin", color="CCCCCC"),
    right=Side(style="thin", color="CCCCCC"),
    top=Side(style="thin", color="CCCCCC"),
    bottom=Side(style="thin", color="CCCCCC"),
)
header_fill = PatternFill("solid", fgColor="0F766E")
header_font = Font(bold=True, color="FFFFFF")
money_format = "#,##0"

detail_headers = [
    "ت",
    "اسم الزبون",
    "الهاتف",
    "رقم الطلب",
    "رقم التسديد",
    "المبلغ",
    "تاريخ التسديد",
    "وقت الإنشاء (بغداد)",
    "المندوب",
    "المستخدم",
    "ClientPaymentId",
]


def style_header(ws, n: int) -> None:
    for col in range(1, n + 1):
        cell = ws.cell(1, col)
        cell.fill = header_fill
        cell.font = header_font
        cell.alignment = Alignment(horizontal="center", vertical="center")
        cell.border = thin


def autosize(ws, widths: list[int]) -> None:
    for i, w in enumerate(widths, 1):
        ws.column_dimensions[get_column_letter(i)].width = w


summary_rows: list[tuple[str, str, int, float]] = []
for list_name in sorted(rows_by_list.keys()):
    items = rows_by_list[list_name]
    total = sum(float(x["amount"] or 0) for x in items)
    mandob = items[0].get("mandob_name") or ""
    summary_rows.append((list_name, mandob, len(items), total))

for list_name, mandob, cnt, total in summary_rows:
    ws0.append([list_name, mandob, cnt, total])

grand_cnt = sum(r[2] for r in summary_rows)
grand_amt = sum(r[3] for r in summary_rows)
ws0.append(["الإجمالي", "", grand_cnt, grand_amt])
style_header(ws0, 4)
for row in ws0.iter_rows(min_row=2, max_row=ws0.max_row, min_col=1, max_col=4):
    for c in row:
        c.border = thin
        c.alignment = Alignment(horizontal="center", vertical="center")
    row[3].number_format = money_format
for c in ws0[ws0.max_row]:
    c.font = Font(bold=True)
autosize(ws0, [28, 24, 16, 16])
ws0.sheet_view.rightToLeft = True


def safe_sheet(name: str) -> str:
    bad = set(r"[]:*?/\\")
    s = "".join("_" if ch in bad else ch for ch in name).strip() or "sheet"
    return s[:31]


used = {"ملخص"}
for list_name in sorted(rows_by_list.keys()):
    base = safe_sheet(list_name)
    title = base
    i = 2
    while title in used:
        suffix = f"_{i}"
        title = base[: 31 - len(suffix)] + suffix
        i += 1
    used.add(title)

    ws = wb.create_sheet(title)
    ws.sheet_view.rightToLeft = True
    ws.append(detail_headers)
    style_header(ws, len(detail_headers))
    items = rows_by_list[list_name]
    for idx, r in enumerate(items, 1):
        amt = float(r["amount"] or 0)
        ws.append(
            [
                idx,
                r.get("customer_name") or "",
                r.get("phone") or "",
                int(r["order_id"]) if r.get("order_id") else "",
                int(r["payment_id"]) if r.get("payment_id") else "",
                amt,
                r.get("payment_date") or "",
                r.get("created_at_baghdad") or "",
                r.get("mandob_name") or "",
                r.get("mandob_user") or "",
                r.get("client_payment_id") or "",
            ]
        )
        row_idx = ws.max_row
        for col in range(1, len(detail_headers) + 1):
            cell = ws.cell(row_idx, col)
            cell.border = thin
            cell.alignment = Alignment(
                horizontal="center", vertical="center", wrap_text=True
            )
        ws.cell(row_idx, 6).number_format = money_format

    total = sum(float(x["amount"] or 0) for x in items)
    ws.append(["", "", "", "", "المجموع", total, "", "", "", "", ""])
    for col in range(1, len(detail_headers) + 1):
        cell = ws.cell(ws.max_row, col)
        cell.font = Font(bold=True)
        cell.border = thin
    ws.cell(ws.max_row, 6).number_format = money_format
    autosize(ws, [6, 28, 14, 12, 12, 12, 14, 20, 20, 12, 38])

wb.save(out)
print(f"OUT={out}")
print(
    f"lists={len(rows_by_list)} payments={sum(len(v) for v in rows_by_list.values())} total={grand_amt:.0f}"
)
for n, m, c, t in summary_rows:
    print(f"  {n}: {c} / {t:.0f}")
