# -*- coding: utf-8 -*-
"""توثيق دفعة 01 — توحيد التنقل وقوالب الصلاحيات."""
from pathlib import Path

from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from docx import Document

ROOT = Path(__file__).resolve().parent
EVID = ROOT / "evidence" / "01-navigation-permissions"
EVID.mkdir(parents=True, exist_ok=True)

HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(color="FFFFFF", bold=True, name="Arial")
BODY_FONT = Font(name="Arial", size=11)
THIN = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)


def style_header(ws, cols):
    for i, h in enumerate(cols, 1):
        c = ws.cell(1, i, h)
        c.fill = HEADER_FILL
        c.font = HEADER_FONT
        c.alignment = Alignment(wrap_text=True, vertical="center")
        c.border = THIN


def append_rows(ws, rows):
    for r in rows:
        ws.append(list(r))
        for cell in ws[ws.max_row]:
            cell.font = BODY_FONT
            cell.alignment = Alignment(wrap_text=True, vertical="top")
            cell.border = THIN


wb = Workbook()

# Endpoints — لا عقود أعمال جديدة؛ عقود الصلاحيات الحالية
ws = wb.active
ws.title = "Endpoints"
style_header(
    ws,
    [
        "Method",
        "Path",
        "DTO",
        "Response",
        "Permissions",
        "Scope",
        "Errors",
        "Consumer",
        "Tests",
        "Status",
    ],
)
append_rows(
    ws,
    [
        (
            "GET",
            "/api/v1/admin/Users/{id}/permission-overrides",
            "userId",
            "UserPermissionOverridesResponse",
            "User.ManagePermissions",
            "user target",
            "403/404",
            "PowersForm",
            "existing permission tests",
            "Source-reviewed / reused",
        ),
        (
            "PATCH",
            "/api/v1/admin/Users/{id}/permission-overrides",
            "PatchPermissionOverridesPayload",
            "updated version",
            "User.ManagePermissions",
            "user target + concurrency",
            "409/403",
            "PowersForm",
            "existing",
            "Source-reviewed / reused",
        ),
        (
            "POST",
            "/api/v1/admin/Auth/login|refresh",
            "credentials/refresh",
            "tokens+permissions+branches",
            "session",
            "branch list",
            "401",
            "Auth store",
            "session revalidation",
            "Source-reviewed / reused",
        ),
        (
            "—",
            "لا API أعمال جديد لهذه الدفعة",
            "—",
            "—",
            "—",
            "—",
            "—",
            "navigation only",
            "RoleDefaultMatrixTests + vitest nav",
            "Implemented",
        ),
    ],
)

ws2 = wb.create_sheet("RoleDefaultMatrix")
style_header(
    ws2,
    [
        "StableId",
        "LegacyRoute",
        "Director",
        "Monitor",
        "SalesManager",
        "PermissionHint",
        "Status",
    ],
)
append_rows(
    ws2,
    [
        ("managementSummary", "director-summary", "Y", "N", "N", "—", "not_implemented"),
        ("managementRatings", "director-ratings", "Y", "Y", "N", "—", "not_implemented"),
        (
            "managementDelegateFollowup",
            "director-delegate-followup",
            "Y",
            "Y",
            "N",
            "DelegateFollowup.Read",
            "ready",
        ),
        ("managementFollowerWork/*", "director-follower-work", "Y", "Y", "N", "—", "not_implemented"),
        (
            "managementTasks",
            "director-assigned-tasks",
            "Y",
            "N",
            "N",
            "Task.Read",
            "ready",
        ),
        ("saleSummary", "sales-manager-sales-summary", "N", "Y", "N", "—", "not_implemented"),
        (
            "saleFollowup",
            "director-sales-followup",
            "Y",
            "Y",
            "N",
            "SalesFollowup.Read",
            "ready",
        ),
        (
            "saleOrders",
            "sales-manager-sales",
            "N",
            "Y",
            "Y",
            "Order.Read",
            "ready",
        ),
        (
            "Roles.Director/Monitor/SalesManager",
            "FE_Company UserType",
            "template",
            "template",
            "template",
            "no ManagePermissions",
            "seed idempotent",
        ),
    ],
)

ws3 = wb.create_sheet("FunctionalMatrix")
style_header(ws3, ["Item", "Status", "Evidence"])
append_rows(
    ws3,
    [
        ("أب لوحة الإدارة + أب المبيعات", "Implemented / Tested", "navigation.registry.test.ts"),
        ("تسميات وظيفية بدل مدير/مراقب", "Implemented", "ar.json + sections labels"),
        ("DelegateFollowup tab = Read only", "Implemented / Tested", "sections.access + registry tests"),
        ("Task tab = Read only (API-aligned)", "Implemented / Tested", "sections.access.test.ts"),
        ("not_implemented ليس قفل صلاحية", "Implemented / Tested", "getReadyNavigationSections filter"),
        ("RoleDefaultMatrix + seed roles", "Implemented / Tested", "RoleDefaultMatrixTests"),
        ("permission dependencies في PowersForm", "Implemented", "permissionDependencies.ts"),
        ("aliases /management/tasks + oversight", "Implemented", "Management/router.ts"),
        ("أعمال الفرع/المفوض محفوظة", "Implemented", "sections branch/delegate ready tabs"),
        ("عمل المتابعين / الملخص / التقييمات", "Blocked / not_implemented", "prompts 02–13"),
        ("مقارنة حية مع FE_Company", "Blocked", "مطابقة مصدرية فقط"),
        ("لقطات 360/768/1440 تفاعلية", "Pending live shell", "evidence/01 — unit tested first"),
    ],
)

ws4 = wb.create_sheet("PermissionsFinal")
style_header(ws4, ["Permission", "FinalName", "In Permissions.All?", "Notes"])
append_rows(
    ws4,
    [
        ("DelegateFollowup.Read", "DelegateFollowup.Read", "No (OutsideAll)", "دخول التبويب"),
        ("DelegateFollowup.UpsertBoardRating", "DelegateFollowup.UpsertBoardRating", "No", "إجراء"),
        ("Task.Read", "Task.Read", "No (OutsideAll)", "دخول قائمة المهام"),
        ("SalesFollowup.Read", "SalesFollowup.Read", "No", "متابعة المبيعات"),
        ("User.ManagePermissions", "User.ManagePermissions", "No (explicit)", "غير ممنوح لقوالب المدير/المراقب"),
    ],
)

xlsx_path = ROOT / "Unified_Admin_Delivery_01_Navigation.xlsx"
wb.save(xlsx_path)

doc = Document()
doc.add_heading("01 — توحيد التنقل وقوالب الصلاحيات", level=1)
doc.add_paragraph(
    "لا API أعمال جديد. أعيد استخدام عقود permission-overrides والجلسة. "
    "الإثبات: مطابقة مصدرية + اختبارات وحدة. لا نشر إنتاج."
)
doc.add_heading("ملفات ومسؤوليات", level=2)
for line in [
    "panel/.../Core/navigation/navigation.registry.ts — السجل المركزي parent/stableId/route/access/status",
    "panel/.../Core/navigation/permissionDependencies.ts — تبعيات توضيحية لمحرر الصلاحيات",
    "panel/.../Management/domain/sections.ts — تبويبات الإدارة؛ Read لدخول المهام/المندوبين",
    "panel/.../Management/router.ts — مسارات جاهزة + aliases آمنة",
    "panel/.../Core/composable/useSideBar.ts — أب يُفتح بابن واحد دون منح الباقي",
    "api/.../SeedData/Roles.cs — أدوار Director/Monitor/SalesManager",
    "api/.../SeedData/RoleDefaultMatrix.cs — مصفوفة القوالب من FE_Company",
    "api/.../SeedData/RolePermissions.cs — seed idempotent للقوالب الجديدة",
    "docs/unified-admin-delivery/01-navigation-field-matrix.md — جدول الحقول قبل التنفيذ",
]:
    doc.add_paragraph(line, style="List Bullet")

doc.add_heading("ما تغيّر", level=2)
doc.add_paragraph(
    "توحيد آباء السايدبار، فصل Read عن الإجراءات، قوالب أدوار جديدة بلا ManagePermissions، "
    "توثيق غير المنفَّذ دون تقديمه كقفل صلاحية."
)
doc.add_heading("المتبقي", level=2)
doc.add_paragraph(
    "تنفيذ مجالات الملخص/التقييمات/عمل المتابعين/… في برومبتات لاحقة. "
    "لقطات تفاعلية على بيئة معزولة عند توفر الجلسة. لا مقارنة حية مع FE_Company."
)
docx_path = ROOT / "Unified_Admin_Delivery_01_Architecture.docx"
doc.save(docx_path)

# coverage md
(ROOT / "Coverage_01_Navigation.md").write_text(
    """# تغطية 01 — التنقل والصلاحيات

| البند | الحالة | الدليل |
|---|---|---|
| أب لوحة الإدارة + أب المبيعات | منفّذ ومختبر | navigation.registry.test.ts |
| تسميات وظيفية | منفّذ | ar.json / sections |
| DelegateFollowup Read-only tab | منفّذ ومختبر | sections.access + registry |
| Task Read-only tab | منفّذ ومختبر | sections.access.test.ts |
| not_implemented ≠ قفل صلاحية | منفّذ ومختبر | getReadyNavigationSections |
| RoleDefaultMatrix + seed | منفّذ ومختبر | RoleDefaultMatrixTests |
| تبعيات PowersForm | منفّذ | permissionDependencies.ts |
| aliases | منفّذ | Management/router.ts |
| أعمال الفرع/المفوض | منفّذ (محفوظ) | sections ready tabs |
| عمل المتابعين/الملخص/… | غير منفّذ | prompts 02+ |
| مقارنة حية FE_Company | غير محسوم/محظور | مطابقة مصدرية |
| لقطات 360/768/1440 | معلّق على جلسة حية | unit أولاً |
""",
    encoding="utf-8",
)

print("OK", xlsx_path.name, docx_path.name)
