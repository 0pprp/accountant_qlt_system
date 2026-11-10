# -*- coding: utf-8 -*-
from pathlib import Path
import shutil
import re

ROOT = Path(__file__).resolve().parent
src = Path(__file__).resolve().parents[1] / "delegate-rebuild" / "generate_docs.py"
dst = ROOT / "generate_docs.py"
shutil.copy2(src, dst)
t = dst.read_text(encoding="utf-8")
t = t.replace("delegate_collector", "delegate_app")
t = t.replace("delegate-rebuild", "delegate-app")
t = t.replace("(أو demo ثابت)", "(إنتاج فقط)")
if "payment_collection_rules" not in t:
    t = t.replace(
        '("features/payments/domain/entities/pending_payment.dart"',
        '("features/payments/domain/payment_collection_rules.dart", "domain", '
        '"قواعد مبلغ التسديد ومنع التكرار اليومي."),\n    '
        '("features/payments/domain/entities/pending_payment.dart"',
    )
# ROOT path: generate_docs uses Path(__file__).parent which is correct for delegate-app
dst.write_text(t, encoding="utf-8")

# Run generator
import importlib.util

spec = importlib.util.spec_from_file_location("gen", dst)
mod = importlib.util.module_from_spec(spec)
spec.loader.exec_module(mod)
excel = mod.write_excel()
word = mod.write_word()
print("EXCEL", excel)
print("WORD", word)

# Per-file Word docs from actual lib tree
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.shared import Pt

APP_LIB = Path(__file__).resolve().parents[2] / "delegate_app" / "lib"
files_dir = ROOT / "architecture" / "files"
files_dir.mkdir(parents=True, exist_ok=True)

# Index
idx = Document()
idx.styles["Normal"].font.name = "Arial"
idx.styles["Normal"].font.size = Pt(11)
h = idx.add_heading("فهرس ملفات تطبيق المندوب (delegate_app)", 0)
h.alignment = WD_ALIGN_PARAGRAPH.RIGHT
p = idx.add_paragraph("برودكشن فقط — Clean Architecture — مسار التوثيق docs/delegate-app")
p.alignment = WD_ALIGN_PARAGRAPH.RIGHT

desc_map = {path: desc for path, layer, desc in mod.ARCHITECTURE_FILES}

count = 0
for dart in sorted(APP_LIB.rglob("*.dart")):
    rel = dart.relative_to(APP_LIB).as_posix()
    layer = "core"
    if rel.startswith("features/"):
        parts = rel.split("/")
        layer = parts[2] if len(parts) > 2 else "feature"
    desc = desc_map.get(rel, f"ملف {rel} ضمن تطبيق المندوب الجديد.")
    doc = Document()
    doc.styles["Normal"].font.name = "Arial"
    title = doc.add_heading(rel, 0)
    title.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    for line in [
        f"الطبقة: {layer}",
        f"المسار الكامل: lib/{rel}",
        f"الوصف: {desc}",
        "التعديل المستقبلي: حدّث هذا الملف مع أي تغيير سلوكي في نفس المسار.",
    ]:
        para = doc.add_paragraph(line)
        para.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    out_name = rel.replace("/", "__").replace(".dart", "_dart.docx")
    doc.save(files_dir / out_name)
    bullet = idx.add_paragraph(f"{rel} — {desc}", style="List Bullet")
    bullet.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    count += 1

idx.save(files_dir / "00_INDEX.docx")
readme = ROOT / "README.md"
readme.write_text(
    """# توثيق تطبيق المندوب (`delegate_app`)

- برودكشن فقط — لا حساب ديمو
- المسار: `accountant_qlt_system/delegate_app`
- `Delegate_API_Documentation.xlsx` — كل API + قبول + فهرس
- `Clean_Architecture_File_Guide.docx` — دليل الطبقات
- `architecture/files/*.docx` — ملف Word لكل مسار كود
- `architecture/files/00_INDEX.docx` — فهرس

عند تعديل أي ملف كود: حدّث Word المقابل وورقة Excel إن تغيّر الـ API.
""",
    encoding="utf-8",
)
print("PER_FILE", count)
print("DOCS_OK")
