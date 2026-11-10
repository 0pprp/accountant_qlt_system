# تأسيس اللوحة الموحدة

## المخرجات
- `Unified_Admin_Foundation.xlsx`
- `Unified_Admin_Foundation_Architecture.docx`
- `screenshots/` — لقطات سطح المكتب والهاتف

## إعادة توليد التوثيق
```bash
pip install openpyxl python-docx
python generate_docs.py
```

## لقطات الشاشة
من مجلد الواجهة بعد `npm run build-only` و`npm run preview -- --host 127.0.0.1 --port 4173`:
```bash
node ../docs/unified-admin-foundation/capture_screenshots.mjs
```
أو نفّذ السكربت عبر `node` من جذر الواجهة حيث تتوفر حزمة `playwright`.
