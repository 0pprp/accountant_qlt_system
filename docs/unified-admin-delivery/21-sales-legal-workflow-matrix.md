# 21 — مصفوفة المتابعة القانونية (دورة عمل)

مصادر: `sales-manager-legal-followups.vue` + `LegalFollowupService` + Cancel الموجود + `SalesRequest.SendToLegalFollowup`.

## جدول الانتقالات

| الحالة الحالية | الإجراء | المنفذ | الشروط | الحالة الجديدة | الأثر |
|---|---|---|---|---|---|
| طلب New بلا هدف / Rejected | إنشاء قضية (إرسال) | مخوّل Create | غير محذوف؛ بلا hold استثناء حي؛ لا قضية InProgress لنفس الطلب؛ اسم/هاتف/عنوان؛ متابع فعّال | Case=InProgress؛ Request=LegalFollowup؛ TargetEmployee=null | Timeline LegalFollowupSent؛ Outbox StatusChanged؛ يظهر في تبويب legal + قائمة المبيعات القانونية |
| Case InProgress | إلغاء | LegalFollowup.Cancel | سبب؛ version؛ فرع مخوّل | Case=Cancelled | إن Request=LegalFollowup → New (غير مسند)؛ لا حذف زبون/دين؛ Timeline Cancelled؛ Outbox |
| Case Cancelled | إلغاء متكرر | — | — | بدون أثر مكرر | idempotent |
| Case InProgress | إكمال ميداني | متابع (تطبيق ميداني) | خارج شاشة المبيعات هذه | Completed | يظهر في تبويب «تمت المتابعة»؛ ليس من واجهة مدير المبيعات القديمة |
| Case Completed | إلغاء | — | مرفوض | — | NotCancellable |

## حقول الشاشة (مصدر)

| العنصر | المصدر | المعنى | الجديد | الصلاحية |
|---|---|---|---|---|
| فرع | SalesBranchFilter | نطاق | branchId | LegalFollowup.Read |
| تبويب InProgress/Completed | tab | حالة القضية | status query | Read |
| صف: زبون/هاتف/متابع/حالة/تاريخ إرسال | list DTO | — | LegalFollowupCases | Read |
| تفاصيل + مستمسكات | detail dialog | معاينة | Get by id + documents file (أُغلق تخزين/معاينة في 22) | Read |
| إنشاء | POST legal-followups من مسار الطلبات | إغلاق فجوة الإنتاج | POST /admin/LegalFollowups | Create |
| إلغاء | غير ظاهر في صفحة المبيعات القديمة؛ موجود للمخرجين/الزبائن القديمة | يُوفَّر عند Cancel | POST .../cancel (عقد حالي) | Cancel |

## علاقة

- ليست `Customer.IsLegal` وحدها.
- الزبائن القديمة للمتابع = قضايا LegalFollowupCases (دفعة 09) — شاشة منفصلة.
- أسماء قانونية لتطبيق الفرع ≠ هذه الشاشة.
- سجل قرارات المفوض ≠ ActivityLog لهذه الدورة.

## أسئلة معزولة

- مستمسكات القضية: أُغلق التخزين والمعاينة في الدفعة 22 (`LegalFollowupCaseDocuments` + Admin/App download + لوحة SalesLegal).
