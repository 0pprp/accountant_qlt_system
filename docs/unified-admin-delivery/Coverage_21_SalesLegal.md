# Coverage 21 — المتابعة القانونية

## الحالة
| المحور | الحكم |
|---|---|
| Implemented | نعم — List/Get/Create/Cancel + `/sale/legal` + إرسال من طلبات البيع + تبويب legal |
| Unit-tested | نعم — CanSend / Cancel / Concurrency / Return idempotent (7) |
| API-live | نعم — `qalaat_20_21_iso` |
| Browser-live | نعم — قائمة قيد العمل تعرض القضية المُنشأة من API |
| Missing | — (أُغلق في 22: معاينة المستمسكات + Complete ميداني للمتابع — انظر Coverage_22) |

## إثبات حي (API)
- Create من طلب New → Case InProgress؛ يظهر في List
- Cancel → Case Cancelled؛ Request Status=New؛ `salesRequestReturnedToUnassigned=true`
- Cancel متكرر → بدون أثر مكرر (`returned=false`)
- الزبون ما زال موجودًا؛ Outbox StatusChanged ×2 (إنشاء+إلغاء) نُشرت
- انظر `evidence/21-sales-legal/verify_effects.sql`

## إثبات حي (Browser)
- `/sale/legal?status=InProgress` — «1 متابعة» + صف زبون قانونية 2
- لقطات: `evidence/21-sales-legal/legal-{360,768,1440}.png`
- التنقل يفصل الشاشة عن أسماء الفرع القانونية وسجل قرارات المفوض

## انتقالات
انظر `21-sales-legal-workflow-matrix.md`. عقد Cancel الحالي محفوظ + Outbox عند إعادة الطلب.

## نواقص سابقة
لم تُغلق بلا إثبات.
