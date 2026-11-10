# Coverage 09 — الزبائن القديمة

## الحالة: منفَّذ قراءة+إلغاء ومختبر وحدويًا — دورة الإنشاء الإنتاجية ناقصة

| بند | الحالة |
|---|---|
| التعريف | LegalFollowupCases للمتابع — ليس IsLegal وحده |
| GET .../old-customers | منفَّذ — FollowerOldCustomers.Read |
| POST LegalFollowups/{id}/cancel | منفَّذ — LegalFollowup.Cancel |
| أثر الإلغاء | Status=Cancelled؛ الطلب LegalFollowup→New؛ بلا حذف زبون/دين |
| إنشاء القضية من تبويب القانونية | **ناقص** — ExtraViews Legal غير منفَّذ؛ اعتماد إدخال موثّق |
| Live UI | Pending |

## فصل الصلاحيات
قراءة القسم لا تلغي. Cancel وحده لا يفتح قائمة غير مخولة.
