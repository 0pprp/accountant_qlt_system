# 09 — مصفوفة مصدر الزبائن القديمة

إثبات: tab `oldCustomers` ← `directorFollowerOpsLegalFollowups` ← `GetLegalFollowupsAsync` ← `LegalFollowupCases` عبر `ListForFollowerAsync`. التعليق المصدري: «زبائن قديمة = قضايا قانونية يرسلها مدير المبيعات للمتابع».

| الحقل/الزر | مصدر الكود | قاعدة العمل | المقابل بالجديد | الصلاحية | اختبار القبول |
|---|---|---|---|---|---|
| تعريف المجموعة | LegalFollowupCases لـ FollowerUserId | ليست IsLegal وحدها وليست «أقدم من تاريخ» | LegalFollowupCase | OldCustomers.Read | فقط قضايا المتابع |
| استثناء الملغاة | Where !IsCancelled | Cancelled لا تظهر في القائمة | فلتر Status≠Cancelled | Read | ملغاة لا تُعرض |
| ترتيب | InProgress قبل Completed | ثم أحدث تاريخ | نفس الترتيب | Read | |
| الحالة | InProgress/Completed/Cancelled | عربي: قيد العمل / تمت المتابعة / ملغاة | status + statusLabel | Read | |
| هوية | CustomerName/Phone/Address | من القضية (لقطة عند الإرسال) | نفس الحقول | Read | |
| SalesRequestId | ربط الطلب | أصل الإرسال | salesRequestId | Read | |
| CustomerId | اختياري | قد يكون null إن زبون جديد | customerId? | Read | |
| ملاحظة/إكمال | Note / CompletionNote | نص القضية | note / completionNote | Read | |
| إلغاء | CancelAsync + canCancelOldCustomer | مدير/مراقب؛ InProgress فقط؛ سبب إلزامي؛ الطلب → New غير مسند | POST LegalFollowups/{id}/cancel | LegalFollowup.Cancel مستقل عن Read | قراءة لا تلغي |
| أثر مالي | — | لا حذف دين/زبون/طلب | لا أثر مالي مخترع | — | يبقى الزبون والدين |
| IsLegal | Customers.IsLegal | وصف زبون ≠ سجل متابعة | لا يُستخدم كتعريف للمجموعة | — | اختبار فرق |
| إنشاء القضية | CreateAsync من مدير المبيعات | إرسال طلب غير مسند/مرفوض | غير مكتمل كدورة إدخال إنتاجية في هذه الدفعة إن لم يُنفَّذ تبويب القانونية | — | اعتماد ناقص موثّق |

## الإلغاء — تفاصيل

1. Status يجب InProgress.
2. reason مطلوب غير فارغ.
3. UPDATE Status=Cancelled + CompletionNote=reason بشرط InProgress (idempotent إن تغيّرت الحالة).
4. إن SalesRequest بحالة LegalFollowup → New + مسح الإسناد + تاريخ LegalFollowupCancelled.
5. لا DELETE للقضية ولا للزبون.
