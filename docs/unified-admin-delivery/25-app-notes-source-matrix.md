# 25 — مصفوفة مصدر ملاحظات التطبيقات

## تعريف المجال (من المصدر)
صندوق وارد للمدير من تطبيقات الميدان (`DirectorManagerNotes`):
- **من يكتب:** مندوب / متابع / مدير فرع (هوية من الجلسة الموثقة)
- **لمن:** مدير اللوحة (حيدر في المرجع) — ليس المفوض ولا ActivityLog
- **الارتباط:** ملاحظة تشغيلية عن التطبيق/التطوير؛ اختيارية مدينة/فرع؛ Subject+Body
- **حالات:** `Unread` / `Read` فقط (ليست boolean مقروء لكل مستخدم — حالة على الصف)
- **لا رد** في المصدر؛ لا إغلاق متعدد الحالات
- معزولة صراحة عن CentralComplaints وملاحظات المفوض

مرجع: `director-notes.vue` + `DirectorNotesController` + `DirectorManagerNotesService` + `follower_complaint_sheet` (وضع ملاحظة) + `delegate_complaint_sheet`.

## ما لا يُدمج
| كيان | السبب |
|---|---|
| CustomerNotes | ملاحظات زبون |
| FollowerDelegateNotes | ملاحظات مندوب↔متابع |
| ملاحظات مصادقة البيع | مسار pending-sales |
| شكاوى المفوض | مسار مستقل |
| ActivityLog | سجل نظام |

## المقابل الحالي في الجديد
| عنصر | الحالة |
|---|---|
| تبويب `oversight/app-notes` | `ready` — AppNotesPage |
| `POST …/committee/director-notes` | يخزّن عبر CreateAppNote (Follower) |
| `POST /app/AppNotes` | SourceApp من دور الجلسة |
| شكاوى committee | stub منفصل — يبقى خارج 25 |
| تطبيق المندوب | SendManagerNotePage |

## فجوات متبقية بعد التنفيذ
1. API-live على iso
2. Browser-live لقطات
3. Device-verified
4. واجهة مدير فرع (الدور مدعوم في API فقط)
