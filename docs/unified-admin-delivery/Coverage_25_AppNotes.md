# Coverage 25 — ملاحظات التطبيقات

## الحكم
| المحور | الحالة |
|---|---|
| Implemented | نعم — `AppNotes` وارد إدارة Unread/Read؛ معزول عن CustomerNotes / FollowerDelegateNotes / شكاوى المفوض |
| Unit-tested | نعم — Domain idempotent MarkRead؛ Create من الجلسة؛ ترتيب قائمة؛ رفض بلا صلاحية |
| API-live | **PASS (دفعة 32)** — POST `/app/AppNotes` مندوب+متابع؛ GET `/admin/AppNotes` total=2 |
| Browser-live | صفحة `oversight/app-notes` جاهزة — لقطات Missing |
| Device-verified | committee director-note + delegate SendManagerNotePage مربوطان بالكود — جهاز Missing |
| Missing | Browser shots / Device؛ تطبيق مدير فرع UI غير موجود (الدور مدعوم في API) |

## قواعد مثبتة
- الكاتب وSourceApp من السيرفر/الدور — لا انتحال من الجسم
- حالات: Unread → Read فقط؛ تعليم مقروء idempotent على مستوى الصف
- لا رد من اللوحة

## الشاشات
| شاشة | مسار | صلاحية |
|---|---|---|
| لوحة الإدارة ← ملاحظات التطبيقات | `/management/oversight/app-notes` | `AppNote.Read` |
| تطبيق اللجنة ← ملاحظة للمدير | `CommitteeActions.sendDirectorNote` | `AppNote.Create` |
| تطبيق المندوب ← ملاحظة إلى الإدارة | `SendManagerNotePage` | `AppNote.Create` |

## Endpoints
- Admin: list / unread-count / get / read / read-all
- App: `POST /app/committee/director-notes`؛ `POST /app/AppNotes`

لا دفعة 26. لا نشر إنتاج.
