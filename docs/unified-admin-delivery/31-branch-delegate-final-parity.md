# 31 — جرد نهائي: إدارة الفرع × المدير المفوض

تاريخ: 2026-11-09 (إغلاق فجوات + API/Browser-live)  
فصل الآباء محفوظ. لا Flutter فرع/مفوض → Device N/A لتلك التطبيقات.  
زر مندوب الجباية (أول تحصيل): مربوط كوديًا في `delegate_app` — **ليس Device-verified**.

## تطبيق الفرع (10)

| # | تبويب | الوظيفة الفعلية | API | الحكم | اختبار / دليل |
|---|---|---|---|---|---|
| 1 | مهامي | InboxForMe | ManagementTasks | **Implemented** | Unit+nav |
| 2 | أسماء قانونية | LegalNames | LegalNames | **Implemented** | 27 |
| 3 | المتدنية | 15ي ممتاز→سيء/متلكئ | BranchDeclining | **Implemented** | Unit |
| 4 | الزبائن | بحث ≥2 (اسم/هاتف/id) + مندوب/بيع/متبقي/تقييم → ملف | BranchCustomers/search | **Implemented** | API: mandob+ممتاز؛ Browser |
| 5 | المبيعات | Sold؛ قراءة مشتركة؛ سقف 200 كالمصدر | BranchNewSales | **Implemented** | mark-read؛ unread↓؛ Sold ثابت |
| 6 | المبيعات الخاطئة | ≤6 متابع + مندوب أول تحصيل | WrongSales + app/first-collection-eval | **Implemented** | mandob→mailbox→read→review→conflict |
| 7 | التقييم | Stats ≡ ManagementRatings | ManagementRatings | **Implemented** | trend endValue=10 ممتاز |
| 8 | التسديدات | صفوف+مجموع لقائمة/فترة قراءة فقط | BranchPayments | **Implemented** | rows+sum=25000 |
| 9 | التنازل | Waivers branch | Waiver | **Implemented** | |
| 10 | سجل القرارات | CustomerDecision | CustomerDecision | **Implemented** | |

زيارة ويب `/management/branch/visit-workspace`: Members لقطة BM؛ AssignedTasks حية؛ مرفقات اختيارية jpg/png/gif/webp/pdf ≤20MB + sniff محتوى.

## تطبيق المفوض (10)

| # | تبويب | الحكم |
|---|---|---|
| 1 الرئيسية | **Implemented** |
| 2 مهامي | **Implemented** |
| 3 التقييمات | **Implemented** (ManagementRatings) |
| 4 الشكاوى | **Implemented** |
| 5 الاستثناءات | **Implemented** |
| 6 المبيعات الخاطئة | **Implemented** |
| 7 التنازل | **Implemented** |
| 8 حسابات الفرع | **Implemented** بلا ManagePermissions |
| 9 أعمال الفرع | **Implemented** سجل زيارات+مرفقات |
| 10 الإعدادات | **Implemented** |

## مقارنة تقييمات الفرع/المفوض ↔ ManagementRatings

| مؤشر مرجعي | التعريف | الفترة | المجموعة | المعادلة/النتيجة الحية | المقابل الجديد |
|---|---|---|---|---|---|
| كتالوج | قوائم/متابعون/موظفون للفرع | — | BranchId | lists=[قائمة تحقق] followers=[متابع تحقق] | `GET ManagementRatings/catalog?branchId=` |
| اتجاه التقييم | تقييم زبائن مسددين (آخر تسديد≤30ي) | من–إلى (مستقل جزئيًا) | فرع | endValue=10، label=ممتاز، customerCount=1 من دفعة iso-live-31=25000 | `.../trend` |
| مجموع تسديدات قائمة | مجموع مبلغ + عدد | من–إلى | listId | totalAmount=25000 paymentCount=1 | `.../list-payments-sum` |
| بطاقات زبائن حسب مستوى | تفاصيل مستوى | من–إلى | scope+level | DetailsRead | `.../customers` |

تقييم الزيارة (0–10 أثناء Open/Depart) **منفصل** عن تقرير الأداء أعلاه.

## NewSales سقف 200

المصدر: `TOP 200` بلا صفحة تالية. السقف حد الاستجابة الكامل في المصدر أيضًا — ليس إخفاءً بعقد pagination متخيل. الأحدث أولًا.

## WrongSale مندوب الجباية

- الحدث: تقييم محل عند أول تحصيل لمبيع جديد (`NewSaleListLogic`).
- API: `POST /api/v1/app/WrongSales/first-collection-eval` — الكاتب من الجلسة؛ ListId=تكليف المندوب؛ SourceApp=`mandob-first-collection`.
- التقييم 1–10 مقبول؛ Notice فقط عند ≤6؛ ≥7 نجاح بلا Notice.
- Flutter: `showShopEvaluationDialog` قبل التحصيل في `collect_payment_sheet.dart`.
- مسار المتابع ≤6 يبقى عبر مصادقة اللجنة — مصدر مختلف، نفس WrongSaleNotice.

## إثبات فتح الزيارة (حي)

| سيناريو | نتيجة |
|---|---|
| فتح أول | Visit id + AssignedTasks حية |
| إعادة نفس الفرع | نفس id؛ لا أعضاء مكررة؛ لا زيارة جديدة |
| فرع آخر مع زيارة نشطة | AlreadyOpen (400) |
| خارج النطاق | BranchScope (400) |
| Members | بعد seed bm_iso: userId=4 «مدير فرع تحقق» |
| Rate→Depart+PNG | Departed + attachment؛ exe مرفوض 400 |
| re-depart | 200 idempotent بلا مرفقات مكررة |
| تنزيل مرفق | admin 200؛ follower 403 |

## هجرات iso

- `20261109182501_Branch31ComplaintsVisitsAndOrderRead`
- `20261109184704_Branch31VisitMembersAttachmentsAndWrongSaleSource`
