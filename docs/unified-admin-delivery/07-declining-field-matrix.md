# 07 — مصفوفة حقول المتدنية

| اسم العرض | المصدر القديم | المعنى | مصدر الجديد | الصلاحية | اختبار القبول |
|---|---|---|---|---|---|
| عنوان القسم | tab declining | المتدنية | FollowerDecliningPage | FollowerDeclining.Read | يظهر بعد اختيار متابع |
| تلميح القاعدة | dfw-hint 7 أيام / درجتين | شرح التصنيف | lookbackRule + targetTransitions | Read | نص واضح بلا tooltip فقط |
| اسم المتابع | سياق الصفحة | هوية | followerName / query | Read (هوية دنيا) | يطابق المختار |
| المحافظة | cityValue | نطاق | branchId | فرع مسموح | لا فرع آخر |
| اسم الزبون | CustomerName | هوية السجل | items[].customerName | Read | أول عمود/بطاقة |
| الهاتف | Phone | تواصل | phone | Read | في التفاصيل |
| القائمة | ListName | تكليف البيع | listName / listId | Read | يظهر |
| من | FromLabel | تقييم سابق | fromLabel + fromScore | Read | نص + درجة |
| إلى | ToLabel / RatingLabel | تقييم حالي | toLabel / ratingLabel + toScore | Read | نص + درجة |
| الأيام | DaysBetween | فرق CaptureDate | daysBetween | Read | 0..7 للسجلات المعروضة |
| تاريخ سابق | PriorCaptureDate (تطبيق) | يوم اللقطة السابقة | priorCaptureDate | Read | ليس CreatedAt |
| تاريخ حالي | CurrentCaptureDate | يوم اللقطة الحالية | currentCaptureDate | Read | |
| الاتجاه | مشتق UI | تراجع عند فرق سالب للدرجة | scoreDelta / declineDirectionLabel | Read | نص+أيقونة+لون؛ السالب = تراجع درجة |
| ملاحظة؟ | HasNote | وجود ملاحظة | hasNote=null حتى 08 | — | يعرض «غير متوفر» لا «لا» كذبًا |
| مصادقة بيع؟ | HasSaleAuth | وجود مصادقة | hasSaleAuth من FollowerSaleAuthentications | Read فقط | لا يمنح كتابة مصادقة |
| عداد النتائج | length rows | عدد زبائن متدنية | totalCount | Read | يطابق items بنفس الفلاتر |
| فتح بروفايل | openCustomerProfile | تنقل | CustomerAuditProfile + returnTo | صلاحيات البروفايل | عودة للقسم/صفحة |
| أزرار اعتماد/معالجة | — | غير موجودة بالمصدر | غير منفَّذة | — | لا زر كتابة |

## أزرار المرجع

| زر | نوعه | الجديد |
|---|---|---|
| بطاقة الاسم | فتح تفاصيل/بروفايل مختصر | اختيار صف + تفاصيل |
| إغلاق البروفايل | تنقل UI | إغلاق التفاصيل |
| بروفايل كامل | تنقل لصفحة زبون | مع returnTo allowlist |

لا فلتر فترة اختيارية بالمصدر — الفترة ثابتة 7 أيام على السيرفر.
