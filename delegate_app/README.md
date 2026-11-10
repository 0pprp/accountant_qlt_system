# مندوب التحصيل (`delegate_app`)

تطبيق Flutter جديد بـ **Clean Architecture** — **برودكشن فقط** (لا ديمو).

## التوثيق

| ملف | المحتوى |
|---|---|
| `docs/delegate-app/Delegate_API_Documentation.xlsx` | كل API + مصفوفة قبول + فهرس الملفات |
| `docs/delegate-app/Clean_Architecture_File_Guide.docx` | شرح الطبقات |
| `docs/delegate-app/architecture/files/*.docx` | ملف Word لكل مسار كود |

## التشغيل

```bash
cd delegate_app
flutter pub get
flutter test
flutter run -d chrome
flutter build apk --release
```

## الطبقات

- `presentation` → شاشات فقط
- `domain` → كيانات + use cases + واجهات
- `data` → Dio + SQLite + تنفيذ المستودعات
- `core` → أمان، شبكة، ثيم، موقع، وقت بغداد
