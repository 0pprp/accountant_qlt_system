# -*- coding: utf-8 -*-
"""يولّد أوراق Tabs / EndpointMap / RoleDefaults / MigrationReview / Acceptance لدفعة Panel.Tab."""
from pathlib import Path

from openpyxl import load_workbook, Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from openpyxl.utils import get_column_letter

ROOT = Path(__file__).resolve().parent
XLSX = ROOT / "Unified_Admin_Permissions.xlsx"
HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(color="FFFFFF", bold=True, name="Arial")
BODY_FONT = Font(name="Arial", size=10)
THIN = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)

# عيّنة ممثّلة من الكتالوج — المصدر الكامل: AdminPanelTabs.cs
TABS = [
    ("home", None, "الرئيسية", "ready", "", "/"),
    ("branch", None, "الفروع", "ready", "Branch.Read|Create|Update;Province.Read", "/branch"),
    ("user", None, "المستخدمون", "ready", "User.Read|Create|Update;Role.Read — بلا ManagePermissions", "/user"),
    ("saleOverview", "sale", "نظرة عامة للمبيعات", "ready", "SalesOverview.Read", "/sale/overview"),
    ("sr.unassigned", "saleRequests", "غير مسند", "ready", "SalesRequest.ReadUnassigned+Assign+…", "/sale/requests?tab=unassigned"),
    ("sr.incoming", "saleRequests", "واردة", "ready", "SalesRequest.TabIncoming+Assign/Reject/…", "/sale/requests?tab=incoming"),
    ("sr.exceptions", "saleRequests", "استثناءات", "ready", "TabExceptions+ExceptionDecide", "/sale/requests?tab=exceptions"),
    ("sr.deleted", "saleRequests", "محذوفة", "ready", "TabDeleted+Restore", "/sale/requests?tab=deleted"),
    ("followerCollection", "managementFollowerWork", "جباية المتابع", "ready", "FollowerCollection.Read+ManageEnabled", "/management/follower-work/collection"),
    ("app-branch-wrongSalesBranch", "managementBranch", "مبيعات خاطئة فرع", "ready", "WrongSale.Read+MarkBranchReviewed", "/management/branch/wrong-sales?mailbox=unreviewed"),
    ("app-delegate-wrongSalesDelegate", "managementDelegate", "مبيعات خاطئة مفوض", "ready", "WrongSale.Read+MarkDelegateRead", "/management/delegate/wrong-sales?mailbox=unread"),
    ("committeeRatings", "managementDelegate", "تقييمات اللجنة", "not_implemented", "", "/management/delegate/committee-ratings"),
]

ENDPOINTS = [
    ("GET", "/api/v1/admin/Users/{id}/tab-access", "User.ManagePermissions", "Panel.Tab.* catalog", "account", "AC-tab-get"),
    ("PATCH", "/api/v1/admin/Users/{id}/tab-access", "User.ManagePermissions + expectedVersion", "Panel.Tab allow/deny/inherit", "account", "AC-tab-patch-409-noop"),
    ("PATCH", "/api/v1/admin/Users/{id}/permission-overrides", "User.ManagePermissions", "legacy — يرفض Panel.Tab والحزم المُدارة", "account", "AC-legacy-reject-panel"),
    ("PUT", "/api/v1/admin/Users/{id}/permissions", "User.ManagePermissions", "legacy — يرفض Panel.Tab والحزم المُدارة", "account", "AC-legacy-put-reject"),
    ("GET", "/api/v1/admin/SalesRequests?tab=unassigned", "SalesRequest.ReadUnassigned أو Panel.Tab.sr.unassigned", "branch+status", "لا يسرّب deleted", "AC-sr-unassigned"),
    ("GET", "/api/v1/admin/SalesRequests?tab=deleted", "SalesRequest.TabDeleted أو Panel.Tab.sr.deleted", "branch+status", "مقفول بدون التبويب", "AC-sr-deleted"),
    ("POST", "/api/v1/admin/panel-tab-migration/dry-run", "User.ManagePermissions + !Production", "snapshot+review", "iso DB", "AC-migrate-dry"),
    ("POST", "/api/v1/admin/panel-tab-migration/apply?confirmIsolated=true", "User.ManagePermissions + !Production", "writes Panel.Tab overrides", "iso DB only", "AC-migrate-apply"),
]

ROLE_DEFAULTS = [
    ("Director", "Panel.Tab.management* + follower* + sale* + app-* + sr.*", "بدون User.ManagePermissions"),
    ("Monitor", "Panel.Tab.sale* + follower* + managementRatings/DelegateFollowup/…", "بدون managementCommitteeTasks"),
    ("SalesManager", "Panel.Tab.sale* + sr.* + notifications/user/customer", "بدون managementDelegateFollowup"),
    ("Admin", "Permissions.All + OutsideAll + كل Panel.Tab.*", "محمي — لا tab-access edit"),
]

MIGRATION_REVIEW = [
    ("open", "كان يملك كامل GrantedActions للتبويب", "Allow فوق دور بلا تبويب أو Inherit إن الدور يفتحه"),
    ("closed", "بلا أي إجراء من الحزمة", "Deny فوق دور يفتح التبويب أو Inherit"),
    ("review", "وصول جزئي سابقًا", "يُقفل + توثيق Owned/WouldGain — قرار يدوي لاحقًا"),
]

ACCEPTANCE = [
    ("AC-catalog-ready", "تبويبات جاهزة فقط في ReadyTabs", "unit", "RoleDefaultMatrixTests / AdminPanelTabs"),
    ("AC-expand", "فتح تبويب يوسّع الإجراءات", "unit", "EffectivePermissionCalculatorTests"),
    ("AC-deny-expand", "قفل تبويب يمنع التوسيع", "unit", "EffectivePermissionCalculatorTests"),
    ("AC-tab-get", "GET tab-access", "unit", "TabAccessHandlerTests"),
    ("AC-tab-patch", "Deny/reset/noop/409/not_ready/duplicate", "unit", "TabAccessHandlerTests"),
    ("AC-legacy-lock", "PATCH overrides يرفض Panel.Tab", "unit", "TabAccessHandlerTests"),
    ("AC-user-no-manage", "تبويب المستخدمون بلا ManagePermissions", "unit", "RoleDefaultMatrixTests"),
    ("AC-migrate-dry", "dry-run على qalaat_22_23_iso", "api-live-iso", "panel-tab-migration/dry-run"),
    ("AC-no-prod-deploy", "لا نشر إنتاج في هذه الدفعة", "process", "explicit"),
    ("AC-ui-editor", "محرر تبويبات RTL + مسودة/حفظ/تراجع", "unit+build", "tabAccessFormState.test + PowersForm.vue"),
    ("AC-ui-nav-panelTab", "سايدبار/guard على PanelTab.*", "unit", "panelTabAccess.test.ts"),
    ("AC-ui-tsc-build", "vue-tsc + vite build", "ci-local", "exit 0"),
    ("AC-ui-create-retry", "إنشاء حساب ثم إعادة حفظ التخصيص", "code", "UserCreateForm.vue"),
    ("AC-ui-409", "تعارض 409 يبقي baseline والمسودة", "unit", "tabAccessFormState.test"),
    ("AC-ui-iso-live", "إثبات متصفح على المعزولة", "browser-iso", "معلّق — PANEL_TAB_BATCH01_UI.md"),
]


def style(ws, headers):
    ws.append(headers)
    for col, _ in enumerate(headers, 1):
        c = ws.cell(1, col)
        c.fill = HEADER_FILL
        c.font = HEADER_FONT
    for row in ws.iter_rows(min_row=1, max_row=ws.max_row, max_col=len(headers)):
        for cell in row:
            cell.border = THIN
            if cell.row > 1:
                cell.font = BODY_FONT
            cell.alignment = Alignment(wrap_text=True, vertical="top")
    for col in range(1, len(headers) + 1):
        vals = [str(c.value or "") for c in ws[get_column_letter(col)]]
        ws.column_dimensions[get_column_letter(col)].width = min(max(len(v) for v in vals) + 2, 72)
    ws.sheet_view.rightToLeft = True


def upsert_sheet(wb, title, headers, rows):
    if title in wb.sheetnames:
        del wb[title]
    ws = wb.create_sheet(title)
    ws.append(headers)
    for r in rows:
        ws.append(list(r))
    for col, _ in enumerate(headers, 1):
        c = ws.cell(1, col)
        c.fill = HEADER_FILL
        c.font = HEADER_FONT
    for row in ws.iter_rows(min_row=1, max_row=ws.max_row, max_col=len(headers)):
        for cell in row:
            cell.border = THIN
            if cell.row > 1:
                cell.font = BODY_FONT
            cell.alignment = Alignment(wrap_text=True, vertical="top")
    for col in range(1, len(headers) + 1):
        vals = [str(c.value or "") for c in ws[get_column_letter(col)]]
        ws.column_dimensions[get_column_letter(col)].width = min(max(len(v) for v in vals) + 2, 72)
    ws.sheet_view.rightToLeft = True
    return ws


def main():
    if XLSX.exists():
        wb = load_workbook(XLSX)
    else:
        wb = Workbook()
        wb.active.title = "Endpoints"

    upsert_sheet(
        wb,
        "Tabs",
        ["tabKey", "parentKey", "displayAr", "status", "grantedActions", "route"],
        TABS,
    )
    upsert_sheet(
        wb,
        "EndpointMap",
        ["method", "path", "legacyGuard", "newGuard", "resourceScope", "testId"],
        ENDPOINTS,
    )
    upsert_sheet(
        wb,
        "RoleDefaults",
        ["role", "defaultPanelTabs", "notes"],
        ROLE_DEFAULTS,
    )
    upsert_sheet(
        wb,
        "MigrationReview",
        ["decision", "meaning", "overridePolicy"],
        MIGRATION_REVIEW,
    )
    upsert_sheet(
        wb,
        "Acceptance",
        ["id", "scenario", "layer", "evidence"],
        ACCEPTANCE,
    )

    # أضف صفوف tab-access إلى Endpoints إن وُجدت
    if "Endpoints" in wb.sheetnames:
        ws = wb["Endpoints"]
        existing = {(str(r[0].value), str(r[1].value)) for r in ws.iter_rows(min_row=2) if r[0].value}
        for method, path, auth, status in [
            ("GET", "/api/v1/admin/Users/{id}/tab-access", "User.ManagePermissions", "منفّذ — دفعة Panel.Tab"),
            ("PATCH", "/api/v1/admin/Users/{id}/tab-access", "User.ManagePermissions + expectedVersion", "منفّذ — دفعة Panel.Tab"),
        ]:
            if (method, path) not in existing:
                ws.append([method, path, auth, status])

    wb.save(XLSX)
    print(f"Wrote {XLSX}")


if __name__ == "__main__":
    main()
