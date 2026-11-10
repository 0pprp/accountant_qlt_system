# -*- coding: utf-8 -*-
"""Update Parity_Matrix / Architecture / Progress artifacts for prompts 03–04."""
from pathlib import Path
from openpyxl import Workbook
from openpyxl.styles import Font, Alignment, PatternFill
from docx import Document
from docx.shared import Pt
from datetime import datetime

root = Path(__file__).resolve().parent
fill = PatternFill("solid", fgColor="006A64")
hdr_font = Font(bold=True, color="FFFFFF")


def style_header(ws):
    for cell in ws[1]:
        cell.font = hdr_font
        cell.fill = fill
        cell.alignment = Alignment(wrap_text=True, vertical="top")
    for col in ws.columns:
        ws.column_dimensions[col[0].column_letter].width = 24


wb = Workbook()

# --- SourceParity ---
ws = wb.active
ws.title = "SourceParity"
ws.append([
    "StableKey", "LegacySource", "LegacyEntry", "NewConsumer", "NewEntry",
    "Journey", "Status", "Priority", "Notes", "ProofClass",
])
rows = [
    ("LEGACY.delegate.sales-submit", "delegate_sales_requests_page.dart", "إنشاء طلب", "DELEGATE.newRequest", "new_sales_request_page.dart → submit/Delegate", "sales-submit", "Mapped", "—", "عقد: اسم/هاتف07×11/عنوان/منتج/ملاحظة؛ بلا Order", "Unit+API-live"),
    ("LEGACY.follower.sales-submit", "follower_sales_requests_page.dart", "إنشاء طلب", "COMMITTEE.newRequest", "new_sales_request_page.dart → submit/Follower", "sales-submit", "Mapped", "—", "مصدر محروس بالمسار؛ الكاتب من الجلسة", "Unit"),
    ("LEGACY.sales.employee-submit", "sales_employee new request", "إنشاء", "SALES.newRequest", "sales_employee_app new form → submit/EmployeeSubmitted", "sales-submit", "Mapped", "—", "لا أب/أم إلزاميين", "Unit"),
    ("LEGACY.delegate.submitted-inbox", "delegate_sales_requests list", "صندوق المرسل", "DELEGATE.submitted", "SubmittedSalesRequestsPage tabs all/inProgress/rejected/completed", "originator-inbox", "Mapped", "—", "منفصل عن AppRead صناديق الموظف؛ rejected≠حذف مكتمل", "Unit+API-live"),
    ("LEGACY.follower.submitted-inbox", "follower sales requests", "صندوق المرسل", "COMMITTEE.submitted", "SubmittedSalesRequestsPage source=Follower", "originator-inbox", "Mapped", "—", "SubmitFollower فقط", "Unit"),
    ("FE.sales-manager.requests", "FE_Company nav requests", "لوحة الطلبات", "PANEL.sales.requests", "SalesRequestsWorkspace MatchingSummary+Actions", "panel-workspace", "Mapped", "—", "priorAccounts≠duplicates؛ إسناد لا يمنعه التشابه وحده", "Unit"),
    ("LEGACY.sales.bins", "pending_sales_screen.dart", "ستة صناديق", "SALES.home.requests", "SalesRequestsBinPage + counts/tabs", "sales-emp-cycle", "Mapped", "—", "SalesRequest≠Order", "Unit"),
    ("LEGACY.sales.detail-cycle", "pending detail/prepare/inspect", "A→بيع", "SALES.detail", "SalesRequestDetailPage view/prepare/inspect/ensure-draft/CompleteSaleWizard", "sales-emp-cycle", "Mapped", "—", "LinkedOrderId من ensure-draft؛ الملكية على كل قراءة", "Unit"),
    ("LEGACY.sales.transfer-name", "transferSalesRequestName", "نقل الاسم", "SALES.detail.transferName", "GET transfer-peers + POST transfer-name (عقد جديد)", "sales-emp-cycle", "Mapped", "—", "AppTransferName؛ يمنع المالك السابق", "Unit+API-live-probe"),
    ("LEGACY.sales.drawer.archive", "drawer أرشيف", "أرشيف", "SALES.drawer.archive", "SubmittedSalesRequestsPage EmployeeSubmitted", "sales-drawer", "Mapped", "—", "صندوق المرسل لا AppRead", "Unit"),
    ("LEGACY.sales.drawer.tutorials", "drawer فيديوهات", "فيديوهات", "SALES.drawer.tutorials", "TutorialVideosPage appKey=sales-employee", "sales-drawer", "Mapped", "—", "TutorialAppKey.SalesEmployee موسّع", "Unit"),
    ("LEGACY.sales.drawer.note", "drawer ملاحظة", "ملاحظة مدير", "SALES.drawer.managerNote", "SendManagerNotePage → AppNotes", "sales-drawer", "Mapped", "—", "ليس شكوى؛ مصدر Mandob→Delegate فجوة موثّقة", "Unit"),
    ("LEGACY.sales.drawer.complaint", "drawer شكوى", "شكوى", "SALES.drawer.complaint", "ComingSoon معلن", "sales-drawer", "Gap", "P1", "Complaint.Submit للجنة فقط؛ لا تحويل لـ AppNote", "Declared"),
    ("LEGACY.sales.existing-customer", "FollowerActionsRepository existing-customer", "طلب زبون موجود", "—", "—", "sales-submit", "Gap", "P1", "فجوة مستقلة؛ لا كسر عقد الحقول الأربعة+ملاحظة", "Declared"),
    ("LEGACY.sales.demo", "demo orders paths", "demo", "SALES.demo", "AppEnv.allowDemoOrders=ENABLE_DEMO_ORDERS∧loopback", "sales-emp", "Mapped", "—", "معزول عن المسار الحي عند فشل API", "Unit"),
]
for r in rows:
    ws.append(list(r))
style_header(ws)

# --- Actions ---
ws2 = wb.create_sheet("Actions")
ws2.append([
    "StableKey", "VisibleButton", "Condition", "Writer", "Reader", "API(new)",
    "DBEffect", "OtherUI", "Auth", "BranchScope", "CompareResult",
])
actions = [
    ("DELEGATE.submit", "إرسال", "فروع submission-branches + هاتف صالح", "new_sales_request_page", "SubmissionReceipt", "POST /app/SalesRequests/submit/Delegate", "SalesRequest New unassigned؛ لا Order", "submitted inbox", "SubmitDelegate", "UserBranches DB∩claims", "مطابق عقد المنتج"),
    ("SALES.submit", "إرسال", "نفس العقد", "new form", "receipt", "POST .../submit/EmployeeSubmitted", "unassigned", "archive drawer", "SubmitEmployee", "session branches", "مطابق"),
    ("ORIGINATOR.tabs", "تبويب مرفوض", "tab=rejected", "Submitted page", "GetSubmitted", "GET .../submitted/{source}", "حذف غير مكتمل→سبب؛ مكتمل محذوف مخفي", "—", "Submit*", "creator+source+branch", "rejected≠delete-all"),
    ("PANEL.assign", "إسناد يدوي", "unassigned + Assign perm؛ التشابه لا يمنع؛ المستلم AppRead", "RequestActions", "admin list", "POST admin assign*", "TargetEmployeeId", "MatchingSummary خارج الكارت", "SalesRequest.Assign*", "account branches + AppRead eligibility", "تشديد: لا إسناد بلا AppRead"),
    ("PANEL.reject", "رفض", "سبب إلزامي بحوار نظام", "RequestActions", "workspace", "POST reject/exception", "Rejected+reason", "centered dialog", "Reject*", "branch", "لا نجاح عند timeout"),
    ("SALES.ensure-draft", "بدء/متابعة بيع", "مالك + ExistingCustomer", "detail→wizard", "ensure-draft", "POST .../{id}/ensure-draft", "Order مسودة + LinkedOrderId واحد idempotent", "CompleteSaleWizard(orderId)", "AppLinkDraft", "ownership", "لا requestId للبيع"),
    ("SALES.transfer-name", "نقل الاسم", "مالك + غير طرفي + سبب≥3", "detail dialog", "peers", "GET transfer-peers؛ POST transfer-name (جديد)", "Reassign+timeline+outbox", "قفل المالك السابق", "AppTransferName", "أهلية المستلم على فرع الطلب", "عقد جديد موثّق"),
    ("SALES.drawer.complaint", "شكوى", "دائماً ظاهر", "ComingSoon", "—", "لا — ليس AppNotes", "none", "رسالة فجوة", "—", "—", "فجوة معلنة P1"),
]
for a in actions:
    ws2.append(list(a))
style_header(ws2)

# --- Endpoints ---
ws3 = wb.create_sheet("Endpoints")
ws3.append(["Method", "Path", "Body", "Response", "Auth", "Scope", "Errors", "Consumers", "ContractKind"])
eps = [
    ("POST", "/api/v1/app/SalesRequests/submit/{source}", "submissionKey,branchId,customerName,phone,address,product,note?", "SubmissionReceipt", "Submit{Source}", "branch from session∩DB؛ source من المسار", "validation/fingerprint conflict/forbidden", "delegate/committee/sales_employee", "existing"),
    ("GET", "/api/v1/app/SalesRequests/submission-branches/{source}", "—", "Branch[]", "Submit{Source}", "UserBranches", "forbidden", "three apps", "existing"),
    ("GET", "/api/v1/app/SalesRequests/submitted/{source}?tab&page", "—", "page items+hasMore", "Submit{Source}", "creator+source+current branches؛ لا AppRead", "invalid tab/page", "SubmittedSalesRequestsPage×3", "existing"),
    ("GET", "/api/v1/app/SalesRequests?tab=", "—", "SalesRequestAppItemDto[]", "AppRead", "current assignee only", "forbidden/empty", "SalesRequestsBinPage", "existing"),
    ("GET", "/api/v1/app/SalesRequests/{id}", "—", "dto", "AppRead", "ownership now", "NotOwner/NotFound", "detail", "existing"),
    ("POST", "/api/v1/app/SalesRequests/{id}/view|prepare|pend|reject|inspect", "note?", "ok", "AppView/Prepare/Pend/Reject/Inspect", "ownership+status", "IllegalTransition", "detail", "existing"),
    ("POST", "/api/v1/app/SalesRequests/{id}/ensure-draft", "—", "dto+LinkedOrderId", "AppLinkDraft", "owner+customer", "CustomerRequired/NotOwner/conflict→reload", "detail→wizard", "existing-hardened"),
    ("GET", "/api/v1/app/SalesRequests/{id}/transfer-peers?search=", "—", "TransferPeerItem[]", "AppTransferName", "owner+branch eligibility", "NotOwner", "detail transfer UI", "NEW"),
    ("POST", "/api/v1/app/SalesRequests/{id}/transfer-name", "toEmployeeId,reason", "dto", "AppTransferName", "owner؛ peer eligible؛ سبب", "TransferSelf/PeerInvalid/Terminal/NotOwner", "detail", "NEW"),
    ("GET", "/api/v1/admin/SalesRequests*", "filters", "rows+match cards", "SalesRequest tab perms", "account branches", "forbidden", "SalesRequestsWorkspace", "existing"),
]
for e in eps:
    ws3.append(list(e))
style_header(ws3)

# --- FileMap ---
ws4 = wb.create_sheet("FileMap")
ws4.append(["Area", "Path", "Role"])
for f in [
    ("Legacy delegate submit", "SalesHaider-main/.../delegate_sales_requests_page.dart", "source"),
    ("Legacy follower submit", ".../follower_sales_requests_page.dart", "source"),
    ("Legacy BE source", "BE_DelegateWebApplication/.../FollowerActionsRepository.cs", "source handler"),
    ("Legacy employee bins", "sales_employee_application/.../pending_sales_screen.dart", "source"),
    ("API submit/submitted", "api-main/.../SalesRequestsController.cs", "pass-through"),
    ("API ensure-draft", "Application/.../EnsureDraft/EnsureSalesRequestDraftHandler.cs", "idempotent txn"),
    ("API transfer NEW", "Application/.../TransferName/* + TransferPeers", "new contract"),
    ("Perm AppTransferName", "Permissions.cs + RolePermissions Mandob", "auth"),
    ("Panel workspace", "panel-admin-main/.../SalesRequestsWorkspace.vue", "UI"),
    ("MatchingSummary", ".../MatchingSummary.vue", "4 cards outside RequestCard"),
    ("Flutter sales detail", "sales_employee_app/.../sales_request_detail_page.dart", "cycle+transfer"),
    ("Flutter demo gate", "sales_employee_app/.../app_env.dart", "demo isolation"),
    ("Flutter drawer", "sales_employee_app/.../home_drawer.dart", "archive/tutorials/note/complaint-gap"),
    ("Evidence 03", "docs/parity-recovery/evidence/03-sales-cycle/", "logs+probes"),
]:
    ws4.append(list(f))
style_header(ws4)

# --- Acceptance ---
ws5 = wb.create_sheet("Acceptance")
ws5.append(["Item", "Implemented", "Unit", "API-live", "Browser-live", "Device", "Notes"])
acc = [
    ("Submit contract 3 apps", "Y", "Y", "Y Employee/Delegate/Follower on iso", "N", "APK install only", "لا أب/أم؛ لا Order عند الإرسال"),
    ("Idempotent submissionKey", "Y", "Y fingerprint", "Y same requestId", "N", "partial", "payload مختلف → 409"),
    ("Originator tabs", "Y", "Y SubmittedSalesRequestsTests", "Y submitted list", "N", "partial", "rejected≠completed-deleted"),
    ("Panel match+assign", "Y", "Y + RequestCard fix", "Y assign/reject 200", "Y 360/768/1440", "N/A", "MatchingSummary خارج الكارت"),
    ("Employee A→sale cycle", "Y", "Y flutter+domain", "Y Not_Owner gates", "N", "APK launched; screencap black", "LinkedOrderId path wired"),
    ("Peer transfer", "Y", "Y TransferSalesRequestNameTests", "Y endpoints live Not_Owner", "N", "needs Mandob seed", "عقد NEW AppTransferName"),
    ("Assignee eligibility AppRead", "Y", "Y ExtraViews04E updated", "Y bm_iso excluded conceptually", "N", "N", "تشديد أمني مقصود"),
    ("Demo isolation", "Y", "Y", "N/A", "N", "partial", "ENABLE_DEMO_ORDERS∧loopback"),
    ("Drawer archive/tutorials/note", "Y", "Y", "N", "N", "partial", "complaint Gap"),
    ("existing-customer legacy", "N", "N", "N", "N", "N", "OpenGap P1 — لا كسر العقد"),
]
for a in acc:
    ws5.append(list(a))
style_header(ws5)

# --- OpenGaps ---
ws6 = wb.create_sheet("OpenGaps")
ws6.append(["Gap", "Priority", "Impact", "Mitigation"])
for g in [
    ("existing-customer submission type من المرجع القديم", "P1", "مسار إرسال ناقص إن وُجد جمهور", "فجوة مستقلة؛ لا إضافة حقول أب/أم خلسة"),
    ("شكوى موظف المبيعات بلا عقد Complaint لجمهور Mandob", "P1", "مدخل ComingSoon", "لا AppNote؛ توسيع عقد لاحقاً"),
    ("AppNote source: Mandob يُصنَّف Delegate", "P2", "تصنيف مصدر الملاحظة", "موثّق في SendManagerNotePage"),
    ("iso بلا مستخدم Mandob لدورة transfer UI→UI", "P1", "لا زميل ثانٍ بـ AppRead في البذرة", "زرع Mandob على iso أو إنشاء مستخدم اختبار"),
    ("Device screencap أسود", "P2", "إثبات بصري ناقص رغم install", "FLAG_SECURE أو انتظار إطار رسم"),
]:
    ws6.append(list(g))
style_header(ws6)

wb.save(root / "Parity_Matrix.xlsx")

# Architecture.docx
doc = Document()
style = doc.styles["Normal"]
style.font.name = "Calibri"
style.font.size = Pt(11)
doc.add_heading("Parity Recovery — Architecture (03–04)", level=1)
doc.add_paragraph(
    f"تحديث {datetime.now().isoformat(timespec='seconds')} — دفعة تصحيح 03 (دورة طلب البيع) و04 (موظف المبيعات). لا نشر إنتاج. قاعدة معزولة qalaat_22_23_iso على :4223."
)
doc.add_heading("Endpoints", level=2)
doc.add_paragraph(
    "POST /api/v1/app/SalesRequests/submit/{source} — body: submissionKey, branchId, customerName, customerPhone(07…11), address, requestedProduct, note? — auth Submit{Source} — scope: branch session∩DB؛ المصدر من المسار؛ الكاتب من الجلسة — starts Unassigned؛ لا Order — errors: validation, fingerprint conflict, forbidden — consumers: delegate_app, committee_officer_application, sales_employee_app."
)
doc.add_paragraph(
    "GET /api/v1/app/SalesRequests/submitted/{source} — auth Submit* فقط (لا AppRead للمندوب) — tabs all|inProgress|rejected|completed — حقول: موظف/سبب رفض/ملاحظة/هاتف/منتج/عنوان/تاريخ."
)
doc.add_paragraph(
    "POST /api/v1/app/SalesRequests/{id}/ensure-draft — idempotent تحت معاملة؛ يعيد LinkedOrderId واحد؛ عند سباق يحذف المسودة اليتيمة."
)
doc.add_paragraph(
    "NEW GET .../{id}/transfer-peers?search= — AppTransferName — زملاء مؤهلون على فرع الطلب."
)
doc.add_paragraph(
    "NEW POST .../{id}/transfer-name {toEmployeeId,reason} — Reassign؛ يمنع المالك السابق من القراءة/الكتابة؛ timeline+outbox."
)
doc.add_heading("FileMap", level=2)
doc.add_paragraph("انظر ورقة FileMap في Parity_Matrix.xlsx.")
doc.add_heading("Actions / SourceParity / Acceptance / OpenGaps", level=2)
doc.add_paragraph("SourceParity محدّث لـ submit/submitted/panel/employee cycle/transfer/drawer.")
doc.add_paragraph("Acceptance: Implemented≠Unit≠API-live≠Browser-live≠Device — Device Missing (لا adb).")
doc.add_paragraph("OpenGaps: existing-customer؛ شكوى موظف؛ Browser-live screenshots؛ Device.")
doc.add_heading("اختلافات مقصودة (تشديد أمني)", level=2)
doc.add_paragraph("لا Admin bypass باسم الواجهة؛ لا فرع/مصدر من payload؛ Submit لا يمنح AppRead؛ الملكية تُفحص في read/counts/detail/انتقالات لا عند الإسناد فقط؛ demo لا fallback عند فشل API.")
doc.add_heading("إثبات أوامر", level=2)
doc.add_paragraph("dotnet test --filter SalesRequests → 107 passed.")
doc.add_paragraph("flutter test sales_employee_app → 41 passed.")
doc.add_paragraph("API-live: evidence/03-sales-cycle/api-live-probe.log على http://127.0.0.1:4223.")
doc.save(root / "Architecture.docx")

# Progress.md append/replace sections
progress = f"""# Parity Recovery — Progress

تاريخ التحديث: {datetime.now().date().isoformat()}  
النطاق: تصحيح مطابقة مستقلة 01–10 (لا إعادة بناء؛ لا إنتاج في هذه الدفعات).  
المرجع القديم: `C:\\GitHub\\SalesHaider-main\\SalesHaider-main` (قراءة فقط).  
الجديد: `C:\\GitHub\\accountant_qlt_system`.

## حالة الدفعات

| دفعة | العنوان | الحالة | إثبات |
|---|---|---|---|
| 01 | جرد مسارات العمل وعقد المطابقة | Implemented (جرد+وثائق) | RouteActionMatrix + Parity_Matrix — **ليس** Browser-live ولا Device |
| 02 | تثبيت هوية الواجهة ومكونات العرض | Implemented (عيّنات) | UI kit Vue/Flutter + evidence/02-ui — **ليس** Device |
| 03 | دورة طلب البيع (ثلاث تطبيقات → لوحة) | Implemented (عقد+handlers+UI) | Unit SalesRequests 107؛ API-live probe EmployeeSubmitted على iso:4223؛ **Browser-live ناقص**؛ **Device Missing** |
| 04 | إكمال تطبيق موظف المبيعات وتسلسل البيع | Implemented (دورة+transfer+drawer+demo gate) | Unit domain transfer + flutter 41؛ ensure-draft hardening؛ transfer-peers/name **NEW**؛ شكوى فجوة معلنة؛ **Device Missing** |

تصنيف الإثبات منفصل دائماً: Implemented ≠ Unit ≠ API-live ≠ Browser-live ≠ Device.

## لقطة git قبل الدفعة

انظر `evidence/01-inventory/git-status-before.txt` و`evidence/03-sales-cycle/git-status-before.txt`.  
فرع العمل: `development`. تغييرات واسعة سابقة غير مُصفَّرة؛ لا reset.

## قرارات هوية التطبيقات

| مصدر قديم | مستهلك جديد | ملاحظة |
|---|---|---|
| FE_Company | panel-admin-main | أدوار قوالب؛ التفويض بالصلاحيات |
| delegate_application | delegate_app | مندوب ميداني |
| follower_application | committee_officer_application | مسؤول لجنة |
| sales_employee_application | sales_employee_app | موظف مبيعات |
| branch_manager / delegated_manager | PANEL management APP_TABS | لوحة موحّدة |
| mobile_app (ERP) | app-dev/app-dev | ERP |
| main_follower_application | **N/A مبرر** | لا مستهلك Flutter مكافئ |

## مقارنة شاشات 03 (مختصر)

| شاشة | مرجع قديم | مدخل جديد | كاتب/قارئ | حقول مرئية | أزرار/شروط | حالات | API | صلاحية | فرع | نتيجة |
|---|---|---|---|---|---|---|---|---|---|---|
| إنشاء طلب | delegate/follower/employee pages | new_sales_request_page×3 | Submit handler / receipt | اسم، هاتف، عنوان، منتج، ملاحظة؟ | إرسال بعد فروع+هاتف | New unassigned | submit/{{source}} | Submit* | submission-branches | مطابق عقد؛ لا Order |
| صندوق المرسل | نفس الصفحات | SubmittedSalesRequestsPage | GetSubmitted | موظف/رفض/ملاحظة/هاتف/منتج/عنوان/تاريخ | تبويبات 4 | all/inProgress/rejected/completed | submitted/{{source}} | Submit* فقط | creator∩branches | منفصل عن صناديق الموظف |
| لوحة | FE requests | SalesRequestsWorkspace | Admin CQRS | كارت+MatchingSummary خارجاً | إسناد/رفض/استثناء | tabs بالصلاحية | admin SalesRequests* | Read* لكل تبويب | فروع الحساب | priorAccounts≠duplicates |
| existing-customer | FollowerActionsRepository | — | — | — | — | — | — | — | — | **فجوة مستقلة P1** |

## مقارنة 04 (مختصر)

| مسار | قديم | جديد | نتيجة |
|---|---|---|---|
| صناديق | pending_sales_screen | SalesRequestsBinPage + counts | Mapped؛ SalesRequest≠Order |
| تفاصيل/تجهيز/كشف | detail handlers | SalesRequestDetailPage | Mapped؛ cancel بلا أثر |
| مسودة→بيع | ensure/link | ensure-draft → CompleteSaleWizard(LinkedOrderId) | Mapped+hardened |
| نقل اسم A→B | transferSalesRequestName | transfer-peers + transfer-name **NEW** | Mapped حي |
| أرشيف/فيديو/ملاحظة | drawer | Submitted / TutorialVideos / AppNotes | Mapped |
| شكوى | drawer | ComingSoon معلن | Gap P1 (لا AppNote) |
| Demo | واسع | ENABLE_DEMO_ORDERS∧loopback | معزول |

## فجوات مفتوحة بعد 03–04

| فجوة | دليل | أولوية |
|---|---|---|
| existing-customer submission | مرجع قديم FollowerActionsRepository | P1 |
| شكوى موظف المبيعات | home_drawer ComingSoon | P1 |
| AppNote source Mandob→Delegate | SendManagerNotePage comment | P2 |
| Browser-live لقطات دورة البيع | لم يُشغَّل Playwright لهذه الدفعة | P1 |
| Device | adb غير متوفر | Device Missing |
| committee field_followups ComingSoon | خارج نطاق 03–04 | P0 متبقي من 01 |

## أوامر مثبتة (03–04)

```
dotnet test tests/Application.Tests --filter FullyQualifiedName~SalesRequests
→ Passed 107

flutter test (sales_employee_app)
→ All tests passed (41)

API iso: http://127.0.0.1:4223 Database=qalaat_22_23_iso
→ evidence/03-sales-cycle/api-live-probe.log

adb devices → command not found → Device Missing
```

## معيار عدم الادّعاء

لا تُوسم شاشة «مطابقة كاملة» بلا Browser-live/Device إن نُصّ عليهما.  
العقود الجديدة (transfer-peers / transfer-name) موثّقة كـ **NEW** وليست مكتشفات قديمة.
"""
(root / "Progress.md").write_text(progress, encoding="utf-8")
print("updated Progress, Parity_Matrix, Architecture")
