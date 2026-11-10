# Coverage 17 — الموقع المباشر لموظفي المبيعات

## الحالة
| المحور | الحكم |
|---|---|
| منفَّذ | نعم — ingest بـ ActorKind + GET latest + `/sale/live-map` + SalesLocation.Read |
| مختبر وحدويًا | نعم |
| مثبت حيًا | ناقص — لا Device-verified |
| ناقص | إثبات دورة جهاز→لوحة؛ خلفية OS غير مدّعاة |
| غير محسوم | لا |

## حد التطبيق
التتبع من تطبيق الموظف **بالمقدمة أثناء الدوام** — ليس إثبات خلفية Android.

## عزل
SalesLocation.Read ≠ FollowerLocation.Read. نقاط SalesEmployee لا تظهر في خريطة المتابعين.
