# Coverage_30 — الفيديوهات التعليمية

تاريخ: 2026-11-09  
لا دفعة 32. لا نشر إنتاج.

## الحكم

| بند | حالة |
|---|---|
| Implemented | نعم — كيان TutorialVideo (رابط خارجي + سؤال) |
| Unit-tested | نعم — TutorialVideoDomainTests |
| API-live | **قائمة PASS (32)** — GET `/admin/TutorialVideos`؛ إنشاء/فتح جهاز Missing |
| Browser-live | **Missing** |
| Device-verified | **Missing** — تشغيل عبر url_launcher خارجي؛ لا مشغّل مضمّن |

## إصلاحات 28/29 المرافقة

| بند | حالة |
|---|---|
| فشل إعلانات → قائمة فارغة | **مصلح** — loading / successEmpty / successData / forbidden / error + إعادة محاولة |
| هوية follower | **مثبت** — `committee_officer_application` = Motaba/follower؛ AnnouncementAppKey يقبل committee/motaba |
| ربط إعلانات اللجنة | **Implemented** في تطبيق اللجنة |
| API-live إعلانات سابق | ما زال مفتوحًا |

## العقود

| Endpoint | صلاحية |
|---|---|
| GET `/admin/TutorialVideos` | Tutorial.Read |
| GET `/admin/TutorialVideos/apps` | Tutorial.Read |
| POST `/admin/TutorialVideos` | Tutorial.Create |
| PUT `/admin/TutorialVideos/{id}` | Tutorial.Update |
| DELETE `/admin/TutorialVideos/{id}` | Tutorial.Delete |
| GET `/app/TutorialVideos/{appKey}` | Tutorial.AppRead |

جمهور: `delegate` | `branch-manager` | `follower`.

## حدود مثبتة من المصدر

- روابط http/https فقط — لا رفع ملف، لا thumbnail، لا فئات، لا تتبع مشاهدة/إكمال.
- لا خلط مع أسئلة تعليمية في شاشات أخرى.
- المستهلكون: delegate_app + committee_officer (follower). لا تطبيق فرع Flutter في الجديد.

## موضع الإدارة

لوحة الإدارة ← الفيديوهات التعليمية (`/management/oversight/tutorials`) — ready بعد القراءة والإدارة الفعليتين.

## Type-check

- قبل: 20 أخطاء موجودة مسبقًا (`evidence/30-tutorial-videos/typecheck-before-errors.txt`)
- بعد: `vue-tsc --noEmit` = 0 في هذه الجلسة (`typecheck-after-full.txt`) — لا أخطاء جديدة من وحدات Tutorial/WrongSale

## هجرة

`20261109180014_TutorialVideos` على `qalaat_22_23_iso` (localhost) — ليس دليل API-live.
