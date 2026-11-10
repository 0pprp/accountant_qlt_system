# Acceptance-04G — إغلاق ربط الواجهات الفعلية

يبني على 04F. **لا يُعلن اكتمال Legal/ingestion ولا Flutter UI على جهاز.**

## فصل الأدلة (إلزامي)

| طبقة | ماذا تثبت | ماذا لا تثبت |
|------|-----------|--------------|
| API / PostgreSQL معزول | عقود الإسناد/الانتقال/ensure-draft/الإكمال | واجهة Vue أو Flutter |
| عميل SignalR مستقل (04F) | Hub+WebSockets+latency من Origin | اتصال `salesRequestRealtime.ts` |
| **Vue panel (هذا الملف)** | حالة «متصل» من العميل الفعلي + تحديث بلا reload | تشغيل تطبيق الموظف |
| Flutter UI على جهاز | — | **Live-blocked** (VS toolchain / web) |
| دورة `app/*` بنفس عقود Flutter | مسار الموظف A/B على القاعدة المعزولة | لقطات شاشة Flutter |

## 1) Vue ↔ Hub

| بند | Status | دليل |
|-----|--------|------|
| تتبع العميل الفعلي + URL | Live-verified | `hubUrl.ts`؛ DEV+loopback → `http://127.0.0.1:5174/hubs/...` عبر Vite proxy |
| توكن حي / تجديد | Implemented | `accessTokenFactory` يقرأ المتجر في كل مرة |
| Negotiate/CORS | Live-verified | جذر المشكلة: MapHub بلا `RequireCors` → Failed to fetch؛ أُصلح بـ `RequireCors` + proxy محلي دون توسيع origins |
| «متصل» فقط عند Connected | Live-verified | لقطة 1440 تظهر «متصل»؛ لا تُعرض عند الانقطاع |
| Polling تعافٍ معلن | Implemented | يبقى عند disconnected/reconnecting |
| تحديث قائمة/عداد/تفاصيل بلا reload | Live-verified | بعد complete #7: sold=2، تفاصيل تم البيع، موظف B، سجل حركة |
| قطع/إعادة وتعويض | Partial Live-verified | reconnect handlers + RefreshSubscriptions؛ جلسة متصلة بعد أحداث متعددة |
| خروج / تبديل حساب | Live-verified جزئيًا | مسح الجلسة → `/auth`؛ لا ادعاء بقاء Hub بعد الخروج |
| لا طباعة tokens | Implemented | `sanitizeConnectError` يُقنّع access_token |

## 2) Flutter على جهاز

| بند | Status | عائق دقيق |
|-----|--------|-----------|
| `API_BASE_URL` / `REQUIRE_HTTPS` عبر dart-define | Implemented | `app_env.dart` |
| Realtime دون اشتراط الدوام | Implemented | `home_tab.dart` يبدأ SignalR من `initState` |
| `flutter run -d windows` | **Live-blocked** | `Unable to find suitable Visual Studio toolchain` |
| منصات windows/web | Implemented (scaffold) | `flutter create . --platforms=windows,web` |
| دورة A/B بنفس عقود التطبيق | Live-verified (API app area) | `04g-live-evidence/flutter_api_cycle.jsonl` — **مفصولة عن UI** |
| ظهور تغييرات التطبيق في اللوحة | Live-verified | اللوحة بقيت «متصل» وانعكس إكمال #7 | 

## 3) التصميم / اللقطات

| بند | Status |
|-----|--------|
| عدم قص حقول الكارت | Live-verified — لقطات 1440/768/360 |
| بيانات متنوعة (اسم طويل، متابع، موظف، ملاحظة، ناقص فرع) | Live-verified — طلب #7 |
| Legal | Not implemented (disabled) |
| ingestion مرسلة | Not implemented |

## 4) ملفات التسليم

- `Acceptance-04G.md` / `.csv` / `.xlsx` / `.docx`
- أدلة: `docs/sales-request-review/04g-live-evidence/`
- لقطات: `04g-live-evidence/screenshots/`

## Go / No-Go

- **Go** لربط Vue الفعلي بالـHub محليًا وتحديث اللوحة بلا reload.
- **No-Go** لاعتبار دورة Flutter المرئية على جهاز مكتملة (toolchain)، وLegal/ingestion.
