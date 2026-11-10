# Coverage 07 — المتدنية (عمل المتابعين)

## الحالة: منفَّذ ومختبر وحدويًا — غير Live-verified

| بند | الحالة |
|---|---|
| قواعد IsDeclining (7ي / −10 / انتقالات) | منفَّذ + اختبارات |
| لقطات FollowerCustomerRatingSnapshots | migration رسمي |
| GET admin/.../declining | منفَّذ — FollowerDeclining.Read |
| تطبيق اللجنة declining-customers | مصحَّح لنفس القاعدة |
| واجهة FollowerDecliningPage | منفَّذ — قراءة فقط |
| تفعيل declining → ready | نعم؛ sales يبقى ready؛ 6 أقسام not_implemented |
| لقطات 360/768/1440 حية | **Pending** — بعد apply migration على قاعدة معزولة |
| ملاحظات HasNote | غير متوفر حتى دفعة08 (موثّق) |
| نواقص دفعة06 | تبقى في سجل 06 — لم تُنقل لمكتمل هنا |

## اختبارات

| نوع | نتيجة |
|---|---|
| FollowerDeclineRulesTests | وحدوية |
| vitest sections + navigation | تحديث ready |
| Live API/UI | غير معلن |

## عقود

`GET /api/v1/admin/FollowerWork/{followerId}/declining?branchId&page&pageSize`

- فشل التفويض/الفرع/المتابع → Error (ليس [] صامت)
- عدم وجود متدنين مع نجاح الحساب → items=[] و totalCount=0
- comparisonMode=stored_snapshots
- notesAvailability=not_implemented

## صلاحيات

- قراءة: `FollowerDeclining.Read`
- لا تمنح: مصادقة بيع، ملاحظات، جباية، موقع، مسارات، كتابة تسديد
