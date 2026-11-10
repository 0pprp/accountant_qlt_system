# 30 — مصفوفة مصدر الفيديوهات التعليمية

مرجع استكشاف: [استكشاف الفيديوهات](f7a4b6d3-3681-4ed2-9fb6-497b8f20bb27).

## الحكم
**روابط خارجية فقط** (http/https) — أسئلة/عناوين عربية + URL. لا رفع ملف، لا صورة مصغرة، لا فئات، لا تتبع مشاهدة/إكمال، لا نطاق فرع.

## النظام الأساسي (Q&A)
`MobileAppTutorialVideos`: `AppKey`, `QuestionAr`, `VideoUrl`, `IsEnabled`, `SortOrder` تلقائي.

جمهور الإدارة: `delegate` | `branch-manager` | `follower`.

المستهلكون: مندوب، مدير فرع، متابع/لجنة (فتح خارجي عبر url_launcher).

## نظام ثانوي (رابط واحد)
`MobileAppTutorialLinks` — صفحة الإدارة القديمة شبه معطّلة؛ لا يُنقل كنموذج موازٍ. يمكن لاحقًا صف واحد لكل appKey ضمن Q&A إن لزم المحاسب.

## المقابل في الجديد
| عنصر | الحالة |
|---|---|
| تبويب الإدارة | stub → يُنفَّذ |
| كيان/API | غير موجود → يُنشأ TutorialVideo |
| delegate_app | رابط YouTube وهمي → قائمة Q&A من API |
| committee_officer (Motaba/follower) | قريبًا → قائمة Q&A appKey=follower |
| تتبع مشاهدة | غير موجود في المصدر — لا يُضاف |
