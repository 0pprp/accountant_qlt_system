# متابعة المبيعات (Director Sales Followup)

نقل لوحة «متابعة المبيعات» من SalesHaider إلى `api-main` + `panel-admin-main` كتبويب فرعي **المبيعات** تحت طلبات البيع.

## المحتويات

| الملف | الغرض |
|--------|--------|
| `SalesFollowup_API_Documentation.xlsx` | خريطة كل endpoint |
| `Clean_Architecture_File_Guide.docx` | الطبقات والاعتمادات |
| `architecture/00_INDEX.docx` | فهرس الملفات |
| `architecture/files/*.docx` | دور كل ملف |
| `rules-and-contracts.md` | قواعد التقييم/المصفّر/التحويلات |
| `SCHEMA_REQUIRED.md` | جداول AppBlock وتقييم المدير |
| `Acceptance.md` | سيناريوهات القبول |
| `generate_docs.py` | مولّد Excel/Word |

```bash
cd docs/director-sales-followup
python generate_docs.py
```

يتطلب: `python-docx`, `openpyxl`.
