# Playwright smoke — delegate_app

تاريخ: 2026-10-22

## الأمر

```bash
cd delegate_app
flutter run -d web-server --web-port=5454 --web-hostname=127.0.0.1
npx playwright-cli open http://127.0.0.1:5454
npx playwright-cli screenshot --filename=login-smoke.png
```

## النتيجة

- عنوان الصفحة: **مندوب التحصيل**
- نص: تسجيل الدخول للإنتاج فقط
- حقول: المحافظة + رمز المندوب + زر تسجيل الدخول
- لقطة: `delegate_app/login-smoke.png`

ملاحظة: جلب المدن من المتصفح قد يفشل بـ CORS؛ جهاز Android يستخدم HTTPS مباشرة بدون CORS.
