# بوابة قبول 10 — Go / No-Go (بدون نشر إنتاج)

تاريخ: 2026-11-10  
HEAD عند الجرد: `650e3a16f66668378c3273d74c9cb7198fdc8e4d` (development) + تغييرات عمل غير مُعلَّمة في الشجرة.  
مرجع OpenGaps: Progress 01–09 + `docs/unified-admin-delivery/32-release-readiness-backlog.md`.

## قرار موجز لكل سطح

| سطح | قرار | أساس | يمنع إنتاج؟ |
|---|---|---|---|
| API iso (qalaat_22_23_iso) | **Conditional Go** | Unit واسعة؛ API-live لشرائح 03–08؛ 09 Excel Unit فقط في هذه الجولة | نعم إن طُلب ادّعاء كامل دون API-live 09 |
| Panel Admin ويب | **Conditional Go** | Browser-live 03/07/08؛ 09 بدون لقطات جديدة في هذه الجولة | نعم للإصدار الرسمي بلا لقطات 09 |
| sales_employee_app | **No-Go Device** | Build سابق؛ Device Missing (لا adb) | نعم لو عُدَّ Device |
| delegate_app | **No-Go Device** | Unit/API-live دفع؛ Device Missing | نعم للوعد الميداني |
| committee_officer_application | **No-Go Device** | API-live 05؛ Device Missing | نعم |
| Realtime/أداء بيانات كبيرة | **No-Go** | غير مُثبت ممثل الحجم على إنتاج | Conditionally |
| Tag/commit نظيف | **No-Go** | working tree متسخ؛ لا tag إصدار | نعم للإصدار الرسمي |

**لا نشر إنتاج في هذه الدفعة.** الحزمة أدناه للتفعيل بعد مراجعة واضحة فقط.

## حسابات معزولة مطلوبة (عقد — ليست بديل صلاحيات)

| حساب مقترح | دور seed | نطاق | ملاحظة |
|---|---|---|---|
| dir_iso | Director | واسع | محتوى+إدارة |
| mon_iso | Monitor | واسع | بلا افتراض Admin |
| sm_iso | SalesManager | فروع مبيعات | بلا ManagePermissions |
| bm_iso | BranchManager | فرع واحد | بوابة زيارة |
| del_iso | (مفوض) | نطاق مفوض | إن وُجد seed |
| se_a / se_b | SalesEmployee + Assignment | فرع مختلف/نفس | إعادة إسناد A→B |
| mandob_iso | Mandob | قائمة | تحصيل |
| motaba_iso | Motaba | قوائم مكلّفة | مصادقة/ميدانية |
| acct_iso | Accountant* | مالي | تنازل Apply |
| limited_iso | صلاحيات ضئيلة | — | 403 متوقع |
| other_branch | نفس الدور فرع آخر | فرع مغاير | عزل نطاق |

الأسماء ليست إثباتًا — وثّق overrides الفعلية عند التشغيل.

## رحلات قبول (مطلوب إعادة تشغيل على iso قبل Go كامل)

1. إنشاء ثلاثة → Unassigned → مطابقة → إسناد/إعادة إسناد → تجهيز/كشف/بيع  
2. مصادقة متابع + WrongSale  
3. دفع مندوب + Sync + تقييم أول  
4. زيارة فرع + مغادرة + أعمال مربوطة بالفرع  
5. مهمة: نقل/إنجاز/تصديق  
6. تنازل: مراجعة → Apply مرة واحدة  
7. قانونية + قرار  
8. نشر PriceBoard/Announcement/Tutorial/AppNote → قراءة تطبيق  
9. سحب صلاحية أثناء الجلسة + تبديل فرع + طلب محظور مباشر  
10. Excel search أسماء + عزل فرع  

Timeout/retry/تعارض اتصالين: راجع أدلة 03/06/23 — أعد على commit المستهدف.

## تصنيف إثبات (لا تخلط)

| وحدة | Implemented | Unit | API-live | Browser-live | Device |
|---|---|---|---|---|---|
| 01 جرد | Y | — | — | — | — |
| 02 UI | Y | — | — | جزئي kit | — |
| 03 طلب بيع | Y | Y | Y | Y 360/768/1440 | Missing |
| 04 موظف مبيعات | Y | Y | Y | جزئي | Missing |
| 05 لجنة | Y | Y | Y | — | Missing |
| 06 مندوب | Y | Y | Y locks | — | Missing |
| 07 إدارة | Y | Y | Y | Y | N/A ويب |
| 08 فرع/مفوض | Y | Y | Y | Y | N/A ويب |
| 09 مبيعات/محتوى | Y | Y (Excel+Tutorial) | Partial هذه الجولة | Missing 09 shots | N/A |
| 10 قبول | Package | — | Aggregate | Aggregate | Missing |

## OpenGaps المجمّعة (لا تُحذف)

- Device لجميع تطبيقات Flutter الميدانية (adb غير متاح في جولات سابقة).  
- Browser-live 09 (overview/excel/content) غير ملتقط في هذه الجلسة.  
- API-live 09 Excel على iso غير مُشغَّل هنا (Unit فقط).  
- **P0 متبقٍ:** واجهة منح/إنهاء `SalesEmployeeAssignment` في Panel (API موجود بلا UI).  
- **P0 متبقٍ:** `sales_employee_app` إعلانات لا تزال seed ثابتة — وعقد AnnouncementAppKey بلا SalesEmployee (قرار منتج).  
- LegalConversion Decide UI؛ exception hold mark-read؛ committeeRatings not_implemented.  
- قياس N+1/حجم كبير على بيانات إنتاجية — غير مثبت.  
- حسابات iso الكاملة (مفوض/مبيعات A-B/محدود) ناقصة تاريخيًا في 32 backlog.  
- Tag نظيف قبل إنتاج.  
- **أُغلق أمنيًا 09 follow-up:** ربط جمهور Announcement/Tutorial العام بهوية الجلسة (`ContentAppAudience` + AudienceDenied).

## حزمة تشغيل معزولة (مسودة تفعيل)

1. فرع/commit محدد + `dotnet ef database update` على iso fresh + upgrade.  
2. Backup قبل التفعيل؛ rollback بيانات = استعادة النسخة — **لا** downgrade آلي لعمليات مالية.  
3. نشر artifacts بـ hashes بدون credentials (انظر plan32 Deployment/Rollback runbooks).  
4. مراقبة Seq اختيارية؛ لا tokens في اللوجات.  
5. قرار التفعيل: جدول الأسطح أعلاه كلها Go أو Conditional موثّق — وإلا No-Go.

## أوامر مثبتة هذه الجلسة

```
dotnet test --filter SalesExcelSearch|TutorialVideos → Passed 11, exit 0
dotnet test --filter SalesOverview|…|PriceBoard|… → Passed 77 (سابقة في الجلسة)
```

Browser/Device: غير مُنفَّذين لهذه الجولة 09/10 — مُسجَّلان Missing لا Passed.
