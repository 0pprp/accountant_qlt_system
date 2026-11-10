# تطبيق موظف المبيعات — التوثيق

هيكل التوثيق مطابق لنمط مندوب التحصيل.

## المحتويات

| الملف | الغرض |
|--------|--------|
| `SalesEmployee_API_Documentation.xlsx` | خريطة كل endpoint (method, path, auth, request/response, قبول) |
| `Clean_Architecture_File_Guide.docx` | طبقات التطبيق وارتباطاتها |
| `architecture/files/*.docx` | وثيقة Word لكل مسار Dart |
| `architecture/00_INDEX.docx` | فهرس ملفات الكود |
| `generate_docs.py` | مولد وثائق Word من شجرة `lib/` |

## تشغيل المولد

```bash
cd docs/sales-employee-app
python generate_docs.py
```

يتطلب: `python-docx`, ويفضّل `openpyxl` لملء الـ Excel.
