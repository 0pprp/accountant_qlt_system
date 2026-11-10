# 04B — تبويبات لوحة المبيعات وحالاتها

يعتمد عقود ومصفوفة [StateTransitions-04A](./StateTransitions-04A.md).

## نطاق التنفيذ

| مجموعة | التبويبات | الحالة في 04B |
|--------|-----------|----------------|
| سير العمل | غير المسند، طلبات البيع، جاهز للبيع، تم الكشف، معلقة، مرفوض، تم البيع | **منفَّذ** |
| عروض إضافية | الكل | **منفَّذ** (اتحاد السجلات المسموح بها فقط) |
| عروض إضافية | المرسلة، الاستثناءات، المحذوفة | **منفَّذة في 04E (API + قراءة اللوحة)** — انظر Acceptance-04.md؛ المرسلة بلا مسار إدخال فعلي |
| عروض إضافية | القانونية | **Not implemented** — مقفلة؛ لا قواعد انتقال إلى LegalFollowup |

لا تُنسخ صفحة غير المسند لكل حالة. مساحة عمل واحدة: `SalesRequestsWorkspace.vue` مع مسار `/sale/requests/unassigned` كغلاف `default-tab=unassigned`.

## صلاحيات التبويب (Permission registry)

مستقلة عن أكشنات Assign / Reject / RequestException:

| تبويب | Permission |
|-------|------------|
| غير المسند | `SalesRequest.ReadUnassigned` |
| طلبات البيع | `SalesRequest.TabIncoming` |
| جاهز للبيع | `SalesRequest.TabPrepared` |
| تم الكشف | `SalesRequest.TabInspected` |
| معلقة | `SalesRequest.TabPending` |
| مرفوض | `SalesRequest.TabRejected` |
| تم البيع | `SalesRequest.TabSold` |
| سجل الحركة | `SalesRequest.TimelineRead` |

- الحاوية تفتح إذا سُمح **تبويب واحد** على الأقل (`anyOf`).
- التبويب المقفل لا يشغّل query.
- «الكل» لا يتجاوز صلاحيات التبويبات؛ يرجع اتحاد السجلات المسموح بها فقط.
- عدادات التبويبات/الفلاتر ضمن نفس النطاق؛ لا تُكشف أعداد التبويبات المحجوبة.
- الاستثناءات والحذف تصنيفات قد تتداخل — **لا** تُجمع عداداتها كإجمالي حالات (غير منفَّذة هنا).

تُمنح عبر `Permissions.SalesRequest.OutsideAll` → دور Admin في `RolePermissions`.

## API إداري

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/v1/admin/SalesRequests?tab=&employeeId=&branchId=&sourceType=&search=&pageIndex=&pageSize=` | جلسة + فحص تبويب في المعالج |
| GET | `/api/v1/admin/SalesRequests/counts?...` | نفس النطاق؛ يعيد فقط التبويبات المسموح بها + `all` |
| GET | `/api/v1/admin/SalesRequests/{id}` | تبويب الطلب الحالي |
| GET | `/api/v1/admin/SalesRequests/{id}/timeline` | TimelineRead + تبويب الطلب |
| GET | `/api/v1/admin/SalesRequests/assignable-employees?branchId=` | Assign |
| GET | `/api/v1/admin/SalesRequests/unassigned` | ReadUnassigned (توافق سابق) |

الترشيح والترتيب والـpagination على السيرفر قبل materialization.

## واجهة

- عنوان «طلبات البيع» + حالة اتصال/آخر مزامنة — بلا بطاقات إحصاء ضخمة.
- شريط تبويبات مع عداد مصرّح ومؤشر لوني؛ «عروض أخرى» للإضافات.
- فلاتر في URL؛ لوحة فلاتر على الهاتف.
- عمود قائمة + تفاصيل على الشاشات الواسعة؛ تفاصيل كاملة مع رجوع على الهاتف.
- منطقة تدقيق واحدة (غير المسند فقط).
- إسناد/إعادة إسناد عبر حوار موظفين مؤهلين — لا إدخال رقم يدوي، لا drag-and-drop للحالة.
- تغيّر حالة الطلب المفتوح يحدّث التفاصيل دون مسح مسودة المستخدم؛ إن خرج من الفلتر تظهر «انتقل إلى …».

## التحقق

- أقفال التبويب، الروابط المباشرة (`?tab=&requestId=`)، العدادات، العودة، تغيّر الحالة مع مسودة مفتوحة.
- لقطات تصميمية 1440 / 768 / 360 عند الحاجة — التحقق البصري لا يثبت API حيًا.
