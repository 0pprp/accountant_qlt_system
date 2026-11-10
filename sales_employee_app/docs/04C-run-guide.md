# 04C — دليل تشغيل تطبيق موظف المبيعات

## ما تغيّر

- تبويبات المنزل تقرأ **عدادات SalesRequests** عبر `GET /api/v1/app/SalesRequests/counts`.
- فتح سلة حالة → `SalesRequestsBinPage` → `SalesRequestDetailPage`.
- `POST .../view` عند الفتح (idempotent).
- إجراءات مقيّدة: تجهيز / تعليق / رفض / كشف / بدء البيع — ليست transfer حرًا.
- `POST .../ensure-draft` قبل معالج الإكمال إن لزم (يتطلب `ExistingCustomerId`).
- فشل الشبكة **لا** يحوّل إلى `demo_orders_store`.

## تشغيل فعلي (جهاز/محاكي)

1. شغّل API مع صلاحيات App* لدور المندوب.
2. من جذر التطبيق:
   ```bash
   cd sales_employee_app
   flutter pub get
   flutter run
   ```
3. سيناريو يدوي مختصر:
   - أسناد طلب من اللوحة إلى الموظف.
   - يظهر في «طلبات البيع» عند الموظف فقط.
   - افتح → اطّلاع → جهّز → علّق/ارفض أو أكمل وفق البيانات.

## Widget / unit tests (ليست إثبات جهاز حي)

```bash
flutter test test/sales_request_tab_map_test.dart
flutter test test/sales_request_actions_test.dart
flutter test test/sales_request_item_test.dart
```

**تمييز:** نجاح `flutter test` يثبت منطق المجال والتحليل؛ التشغيل على جهاز يثبت الربط الحي مع API.
