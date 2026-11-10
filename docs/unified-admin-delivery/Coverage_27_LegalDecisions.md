# Coverage 27 — الأسماء القانونية وطلبات التنازل وسجل القرارات

## الحكم
| المحور | الحالة |
|---|---|
| Implemented | نعم — طابور WeakPayer + قرارات فرع + سجل + ربط طلبات التنازل بمجال 26 + LegalConversion |
| Unit-tested | نعم — WeakPayerRules + تسميات القرارات + تحويل قانونية + قوالب صلاحيات |
| API-live | Missing للدورة المصادقة؛ جداول الهجّرة موجودة على iso المحلي |
| Browser-live | Missing |
| Device-verified | Missing — لا تطبيقات Flutter فرع/مفوض في الجديد؛ العقد API جاهز |
| Missing | واجهة اعتماد LegalConversion في اللوحة (API موجود)؛ API-live/Browser/Device |

## فصل المصطلحات (مثبت)
| مصطلح | كيان | ليس |
|---|---|---|
| الأسماء القانونية | طابور WeakPayerRules | `LegalFollowupCases` |
| قرار إدارة الفرع | `CustomerDecision` | `ActivityLog` |
| طلب قانونية | `LegalConversionRequest` → `IsLegal` | متابعة مبيعات |
| طلب/تنازل | `CustomerWaiverRequest` (دفعة 26) | كيان مستقل عن السجل |
| سجل القرارات | قراءة `CustomerDecision` | ActivityLog العام |

## قرارات الفرع
Continue (+7d) · FakeSale · LegalRequest · WaiverRequest — ملاحظة إلزامية.
موافقة LegalConversion تضبط `IsLegal` وتكتب قرارًا من نوع Legal.

## الشاشات
| شاشة | مسار | صلاحية |
|---|---|---|
| أسماء قانونية | `/management/branch/legal-names` | `LegalNames.Read` (+ Create للقرار) |
| طلبات التنازل (فرع) | `/management/branch/waivers` | `Waiver.Read` |
| سجل القرارات | `/management/branch/decision-history` | `CustomerDecision.Read` |
| طلبات التنازل (مفوض) | `/management/delegate/waivers` | `Waiver.Read` أو `Decide` |

## Endpoints
`GET …/CustomerDecisions/legal-names` · `GET/POST …/CustomerDecisions` · `POST …/legal-conversions/{id}/approve|reject`

لا دفعة 28/29. لا نشر إنتاج. نواقص 10–25 تبقى مفتوحة دون دليل جديد.
