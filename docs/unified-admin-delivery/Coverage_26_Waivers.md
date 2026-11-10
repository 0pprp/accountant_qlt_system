# Coverage 26 — التنازلات

## الحكم
| المحور | الحالة |
|---|---|
| Implemented | نعم — مجال `CustomerWaiverRequest` + API + تبويبات الإدارة/الفرع/المفوض |
| Unit-tested | نعم — 29 اختبارًا (مجال + قواعد + قوالب صلاحيات) ضمن Waivers/CustomerDecisions |
| API-live | **PASS (دفعة 32)** — Pending→Rejected؛ Pending→AwaitingAccountant→Apply→Approved؛ SellAmount 150000→149950؛ إعادة Apply بلا خصم ثانٍ؛ SelfDecide؛ evidence/32-system/waiver-financial.json |
| Browser-live | Missing — صفحات Vue جاهزة دون لقطات 32 |
| Device-verified | Missing — تطبيقات Flutter فرع/مفوض غير موجودة في الجديد |
| Missing | Browser / Device؛ حساب محاسب منفصل عن Admin إن لزم سياسة |

## دورة مثبتة من المصدر
`Pending → Rejected` أو `Pending → AwaitingAccountant → Approved`

| فاعل | صلاحية | إجراء |
|---|---|---|
| فرع | `Waiver.Create` + `Read` | إنشاء ومتابعة (ملاحظة إلزامية) |
| مفوض | `Waiver.Decide` | قبول بمبلغ >0 أو رفض — لا اعتماد ذاتي |
| محاسب | `Waiver.Apply` | خصم `SellAmount` بسقف المتبقي — idempotent |
| إشراف | `Waiver.Read` | قائمة لكل الطلبات |

## الأثر المالي
خصم من `Order.SellAmount` فقط عند Apply — ليس تسديدًا ولا حذف قسط ولا إلغاء بيع ولا إزالة قانونية.

## الشاشات
| شاشة | مسار | صلاحية فتح |
|---|---|---|
| لوحة الإدارة ← التنازلات | `/management/oversight/waivers` | `Waiver.Read` |
| إدارة الفروع ← طلبات التنازل | `/management/branch/waivers?scope=branch` | `Waiver.Read` (+ Create للإجراء) |
| المدير المفوض ← طلبات التنازل | `/management/delegate/waivers?scope=delegate` | `Read` أو `Decide` |

قراءة لا تمنح قرارًا. قرار لا يفتح التبويب وحده دون Read عند الاعتماد على requirement الحالي للمفوض (anyOf).

## Endpoints
`GET/POST /api/v1/admin/Waivers` · `GET …/{id}` · `POST …/approve|reject|apply`

## تحقق بناء
- `dotnet build Api.csproj` — succeeded
- Unit filter Waivers|CustomerDecisions — Passed 29
- Panel `vue-tsc` — أخطاء سابقة خارج وحدات 26/27؛ لا أخطاء جديدة في Waivers/CustomerDecisions

لا دفعة 28. لا نشر إنتاج.
