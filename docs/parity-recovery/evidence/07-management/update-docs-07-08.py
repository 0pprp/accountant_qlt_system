from pathlib import Path
from datetime import datetime, timezone

import openpyxl
from docx import Document

root = Path(r"C:\GitHub\accountant_qlt_system\docs\parity-recovery")
progress = root / "Progress.md"
text = progress.read_text(encoding="utf-8")

# bump header + status table
text = text.replace(
    "تاريخ التحديث: 2026-11-10 (دفعات 05–06 + سلامة التحصيل)",
    "تاريخ التحديث: 2026-11-10 (دفعات 05–08)",
)
if "| 07 |" not in text:
    text = text.replace(
        "| 06 | مندوب جباية + مزامنة + تقييم أول | Implemented + payment-safety | Unit+API-live (idempotency/locks)؛ Device معلّق |",
        "| 06 | مندوب جباية + مزامنة + تقييم أول | Implemented + payment-safety | Unit+API-live (idempotency/locks)؛ Device معلّق |\n"
        "| 07 | لوحة الإدارة/المدير/المراقب + بروفايل | Implemented (harden) | Unit+API-live+Browser-live 360/768/1440؛ Device N/A ويب |\n"
        "| 08 | مدخل إدارة الفروع + المدير المفوض | Implemented (visit-gate) | Unit+API-live+Browser-live؛ Device N/A ويب |",
    )

block_07_08 = """
## دفعة 07 — لوحة الإدارة والمدير والمراقب

- ManagementSummary مستقل عن SalesSummary؛ DelegateFollowup (OrderList.Id، نشاط ≤40ي)؛ ManagementRatings (فئة ≤30ي)؛ FollowerWork 8 أقسام؛ FollowerDeclining=7ي ≠ BranchDeclining=15ي؛ RequiredDue يستثني IsLegal.
- المراقب: لا ManagementSummary افتراضياً (403)؛ منح Read يعمل عبر صلاحية فقط. المدير+المراقب نفس اللوحة.
- Harden: عودة بروفايل التدقيق تسمح بمسارات follower-work + route names؛ إصلاح `params.id` لزبائن متابعة المندوبين؛ رابط بروفايل من التقييمات؛ تنقّل أقسام المتابعين يحافظ على branchId/followerId.
- API-live (`evidence/07-management/api-live-07.log`): SUMMARY_MATRIX؛ MONITOR_SUMMARY=403؛ MONITOR_RATINGS=OK؛ BM_SUMMARY=403.
- Browser-live: summary-360/768/1440.png.

## دفعة 08 — إدارة الفروع والمدير المفوض

- أبوان مستقلان APP_TABS 10×10 تطابق home_shell القديم؛ لا تكرار موديولات.
- بوابة زيارة لأعمال `/management/branch` فقط (EntryRedirect + useBranchVisitGate) — لا تُفرض على oversight/تقارير عامة.
- OpenBranchVisit: idempotent لنفس الفرع؛ فرع آخر → AlreadyOpen/400. BM seed يحصل Task.Read+إجراءات inbox.
- API-live (`evidence/08-branch-delegate/api-live-08.log`): OPEN2_IDEMPOTENT؛ OTHER_BRANCH=400؛ WrongSales/NewSales/Tasks OK.
- Browser-live: branch-visit-768.png (إعادة توجيه لـ visit-workspace بلا زيارة نشطة).
- فجوات مفتوحة موثّقة: LegalConversion Decide UI؛ exception hold mark-read؛ committeeRatings not_implemented؛ لا Device لهذه الوحدات الويب.

"""

if "## دفعة 07" not in text:
    text = text.replace("## فجوات متبقية", block_07_08 + "## فجوات متبقية")

# keep prior open gaps + add new ones
if "LegalConversion Decide UI" not in text:
    text = text.replace(
        "| إثبات Device لوحات مفاتيح 360/412 | معلّق (لا جهاز/adb) | 05–06 |",
        "| إثبات Device لوحات مفاتيح 360/412 | معلّق (لا جهاز/adb) | 05–06 |\n"
        "| LegalConversion Decide UI في اللوحة | P1 | 08 |\n"
        "| Exception hold mark-read للمفوض | P2 | 08 |\n"
        "| committeeRatings tab | P2 معلن | 07 |\n"
        "| existing-customer من المرجع القديم | P1 | 03 |",
    )

if "api-live-07.log" not in text:
    text = text.replace(
        "  evidence/06-delegate/api-live-dual-process-locks.log\n",
        "  evidence/06-delegate/api-live-dual-process-locks.log\n"
        "  evidence/07-management/api-live-07.log + summary-360/768/1440.png\n"
        "  evidence/08-branch-delegate/api-live-08.log + branch-visit-768.png\n",
    )

text = text.replace("لا نشر إنتاج. لا بدء 07.", "لا نشر إنتاج. لا بدء 09.")
progress.write_text(text, encoding="utf-8")

wb = openpyxl.load_workbook(root / "Parity_Matrix.xlsx")
ws = wb["05_06_Updates"]
# ensure sheet name stays; append 07/08 rows
existing = " ".join(str(r[0].value or "") + str(r[1].value or "") for r in ws.iter_rows(min_row=2))
rows = []
if "07" not in existing or "ManagementSummary" not in existing:
    rows.append(
        [
            "07",
            "Oversight summary/ratings/delegate-followup/follower-work/profile",
            "FE director-*",
            "ManagementSummary/Ratings/DelegateFollowup/FollowerWork + audit profile return",
            "Y",
            "Y",
            "Y",
            "Y",
            "N/A",
            "Monitor no default summary; explicit Read; OrderList.Id; 40/30/7/15 rules",
        ]
    )
if "08" not in existing or "visit-gate" not in existing:
    rows.append(
        [
            "08",
            "Branch + Delegated manager parents + visit gate",
            "BM/DM home_shell",
            "APP_TABS + BranchVisits open/idempotent/reject-other",
            "Y",
            "Y",
            "Y",
            "Partial",
            "N/A",
            "visit required for branch ops only; LegalConversion UI open gap",
        ]
    )
for r in rows:
    ws.append(r)
wb.save(root / "Parity_Matrix.xlsx")

doc = Document(root / "Architecture.docx")
doc.add_heading("Parity recovery 07–08 — Management & Branch/Delegate", level=1)
doc.add_paragraph(
    f"Updated {datetime.now(timezone.utc).isoformat()} — iso only; no production; no batch 09."
)
doc.add_heading("Endpoints (07)", level=2)
for line in [
    "GET /api/v1/admin/ManagementSummary/matrix|details — Read(+Drilldown)",
    "GET /api/v1/admin/DelegateFollowup/lists|summary|customers — OrderList.Id; ≤40d activity",
    "GET /api/v1/admin/ManagementRatings/* — active rating class ≤30d",
    "GET /api/v1/admin/FollowerWork/* + FollowerTracking live/route — 8 sections",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("Endpoints (08)", level=2)
for line in [
    "POST /api/v1/admin/BranchVisits/open — idempotent same branch; other→AlreadyOpen",
    "GET /api/v1/admin/BranchVisits/active — per actor",
    "POST .../ratings|depart — note+attachments",
    "Admin ManagementTasks?inboxForMe; WrongSales; BranchNewSales mark-read",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("FileMap", level=2)
for line in [
    "panel: ManagementWorkspace + useBranchVisitGate; useAuditReturnContext allowlist; FollowerSectionNavigation; ManagementRatings profile link",
    "API: RolePermissions BranchManager Task.Read+inbox actions; BranchVisitCommands Open idempotent",
    "evidence: 07-management/*, 08-branch-delegate/*",
]:
    doc.add_paragraph(line, style="List Bullet")
doc.add_heading("OpenGaps retained", level=2)
doc.add_paragraph(
    "03 existing-customer; Device gaps 03–06; LegalConversion Decide UI; exception hold mark-read; committeeRatings not_implemented; no Device claim for web 07/08."
)
doc.save(root / "Architecture.docx")
print("DOCS_OK")
