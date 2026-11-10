from openpyxl import Workbook
from docx import Document
from pathlib import Path

root = Path(__file__).resolve().parent
(root / "evidence" / "30-tutorial-videos").mkdir(parents=True, exist_ok=True)
(root / "evidence" / "31-branch-delegate").mkdir(parents=True, exist_ok=True)

wb = Workbook()
ws = wb.active
ws.title = "Endpoints"
ws.append(["Area", "Method", "Path", "Permission", "Consumer", "Status"])
for r in [
    ("30", "GET", "/api/v1/admin/TutorialVideos", "Tutorial.Read", "panel oversight", "Implemented"),
    ("30", "GET", "/api/v1/admin/TutorialVideos/apps", "Tutorial.Read", "panel", "Implemented"),
    ("30", "POST", "/api/v1/admin/TutorialVideos", "Tutorial.Create", "panel Director", "Implemented"),
    ("30", "PUT", "/api/v1/admin/TutorialVideos/{id}", "Tutorial.Update", "panel", "Implemented"),
    ("30", "DELETE", "/api/v1/admin/TutorialVideos/{id}", "Tutorial.Delete", "panel", "Implemented"),
    ("30", "GET", "/api/v1/app/TutorialVideos/{appKey}", "Tutorial.AppRead", "delegate+committee", "Implemented"),
    ("29-fix", "GET", "/api/v1/app/Announcements/follower", "Announcement.AppRead", "committee_officer", "Implemented"),
    ("31", "GET", "/api/v1/admin/WrongSales", "WrongSale.Read", "branch+delegate panel", "Implemented"),
    ("31", "POST", "/api/v1/admin/WrongSales/{id}/mark-delegate-read", "WrongSale.MarkDelegateRead", "delegate", "Implemented"),
    ("31", "POST", "/api/v1/admin/WrongSales/{id}/mark-branch-reviewed", "WrongSale.MarkBranchReviewed", "branch", "Implemented"),
    ("31", "POST", "/api/v1/app/committee/pending-sales/{orderId}/authenticate", "FollowerSalesReview.Authenticate", "committee → may create WrongSaleNotice", "Implemented"),
]:
    ws.append(r)

ws2 = wb.create_sheet("Fields")
ws2.append(["Entity", "Field", "Notes"])
for r in [
    ("TutorialVideo", "AppKey", "delegate|branch-manager|follower"),
    ("TutorialVideo", "QuestionAr", "required ≤400"),
    ("TutorialVideo", "VideoUrl", "http/https absolute only"),
    ("TutorialVideo", "IsEnabled/SortOrder", "server filter for apps"),
    ("WrongSaleNotice", "OrderId unique", "idempotent intake from auth ≤6"),
    ("WrongSaleNotice", "DelegateReadAtUtc", "read ≠ business status"),
    ("WrongSaleNotice", "BranchManagerReviewedAtUtc", "independent of DM read"),
]:
    ws2.append(r)

ws3 = wb.create_sheet("Permissions")
ws3.append(["Permission", "Templates", "Notes"])
for r in [
    ("Tutorial.* admin", "Director write; Admin all", "OutsideAll"),
    ("Tutorial.AppRead", "Mandob + Motaba", "server filters by appKey"),
    ("WrongSale.Read/Mark*", "Director + Monitor review; Admin all", "OutsideAll; ≠ SalesRequest exception"),
]:
    ws3.append(r)

ws4 = wb.create_sheet("Screens")
ws4.append(["Screen", "Route", "Status"])
for r in [
    ("TutorialVideosPage", "/management/oversight/tutorials", "Implemented"),
    ("WrongSalesPage branch", "/management/branch/wrong-sales", "Implemented"),
    ("WrongSalesPage delegate", "/management/delegate/wrong-sales", "Implemented"),
    ("Branch NewSales", "—", "Partial — SaleList temporary"),
    ("Branch visits", "—", "Missing"),
    ("Delegate complaints", "—", "Missing"),
    ("Delegate home/settings", "—", "Missing"),
    ("BM accounts scoped", "APP shell locked", "Partial"),
]:
    ws4.append(r)

ws5 = wb.create_sheet("Parity31")
ws5.append(["App", "Tab", "Verdict"])
for r in [
    ("branch", "مهامي", "Implemented"),
    ("branch", "أسماء قانونية", "Implemented"),
    ("branch", "الزبائن المتدنية", "Missing"),
    ("branch", "الزبائن", "Partial"),
    ("branch", "المبيعات", "Partial"),
    ("branch", "المبيعات الخاطئة", "Implemented"),
    ("branch", "التقييم", "Missing"),
    ("branch", "التسديدات", "Partial"),
    ("branch", "طلبات التنازل", "Implemented"),
    ("branch", "سجل القرارات", "Implemented"),
    ("delegate", "الرئيسية", "Missing"),
    ("delegate", "مهامي", "Implemented"),
    ("delegate", "التقييمات", "Missing"),
    ("delegate", "الرسائل والشكاوى", "Missing"),
    ("delegate", "طلبات الاستثناء", "Implemented"),
    ("delegate", "المبيعات الخاطئة", "Implemented"),
    ("delegate", "طلبات التنازل", "Implemented"),
    ("delegate", "حسابات إدارة الفرع", "Partial"),
    ("delegate", "أعمال إدارة الفرع", "Missing"),
    ("delegate", "الإعدادات", "Missing"),
]:
    ws5.append(r)

ws6 = wb.create_sheet("Tests")
ws6.append(["Suite", "Result", "Verdict"])
for r in [
    ("TutorialVideoDomainTests", "8 passed (incl. related)", "Unit-tested"),
    ("WrongSaleNoticeTests", "3 passed", "Unit-tested"),
    ("workspaceNavigation + sections.access", "12 passed", "Unit-tested"),
    ("EF TutorialVideos + WrongSaleNotices on iso", "applied", "Schema iso"),
    ("API-live", "-", "Missing"),
    ("Browser-live", "-", "Missing"),
    ("Device-verified", "N/A branch/DM apps absent", "Missing"),
]:
    ws6.append(r)

wb.save(root / "Unified_Admin_Delivery_30_31.xlsx")

doc = Document()
doc.add_heading("Unified Admin Delivery 30-31 Architecture", 0)
doc.add_paragraph(
    "30 Tutorial videos = external URL Q&A per appKey (delegate, branch-manager, follower). "
    "No upload, no watch tracking, no categories. "
    "committee_officer_application is the Motaba/follower consumer (same audience as Announcement follower). "
    "31 integrates branch and delegate workflows without merging parents into admin; "
    "shared domains only when semantics match. WrongSaleNotice closes batch-06 deferred wrong-sale routing."
)
doc.add_heading("Domain boundaries", 1)
for t in [
    "TutorialVideo ≠ educational quiz screens elsewhere",
    "WrongSaleNotice ≠ SalesRequest Hold exception ≠ order reject",
    "AppNote ≠ delegate complaints mailbox",
    "FollowerDeclining ≠ branch declining customers",
    "SaleList ≠ NewSales unread inbox",
    "UserList+ManagePermissions ≠ BM scoped accounts",
    "BmBranchVisits open/rate/depart ≠ switching branchId in panel",
]:
    doc.add_paragraph(t, style="List Bullet")
doc.add_heading("File responsibilities", 1)
for t in [
    "Domain: TutorialVideo; WrongSaleNotice",
    "Application: TutorialVideos CQRS; WrongSales list/mark; AuthenticatePendingSale creates notice when rating≤6",
    "Api: admin/TutorialVideos; app/TutorialVideos; admin/WrongSales",
    "Panel: TutorialVideosPage; WrongSalesPage; workspaceNavigation APP_TABS",
    "Apps: delegate_app tutorials+announcement states; committee announcements+tutorials follower",
]:
    doc.add_paragraph(t, style="List Bullet")
doc.add_heading("Shared reuse rationale", 1)
doc.add_paragraph(
    "Waivers, LegalNames, CustomerDecisions, ManagementTasks, SalesRequests exceptions reused where source semantics match. "
    "Wrong sales get a dedicated notice box because source separates them from Hold exceptions."
)
doc.add_heading("Open gaps", 1)
doc.add_paragraph(
    "See 32-release-readiness-backlog.md and 31-branch-delegate-final-parity.md. "
    "No production publish. Batch 32 not started."
)
doc.save(root / "Unified_Admin_Delivery_30_31_Architecture.docx")
print("ok")
