from openpyxl import Workbook
from docx import Document
from pathlib import Path

root = Path(__file__).resolve().parent
(root / "evidence" / "28-price-board").mkdir(parents=True, exist_ok=True)
(root / "evidence" / "29-announcements").mkdir(parents=True, exist_ok=True)

wb = Workbook()
ws = wb.active
ws.title = "Endpoints"
ws.append(["Area", "Method", "Path", "Permission", "Consumer", "Status"])
for r in [
    ("28", "GET", "/api/v1/admin/PriceBoard", "PriceBoard.Read", "panel oversight", "Implemented"),
    ("28", "POST", "/api/v1/admin/PriceBoard", "PriceBoard.Create", "panel Monitor", "Implemented"),
    ("28", "PUT", "/api/v1/admin/PriceBoard/{id}", "PriceBoard.Update", "panel Monitor", "Implemented"),
    ("28", "DELETE", "/api/v1/admin/PriceBoard/{id}", "PriceBoard.Delete", "panel Monitor", "Implemented"),
    ("28", "GET", "/api/v1/app/PriceBoardAds", "PriceBoard.Read", "sales_employee_app", "Implemented"),
    ("28", "GET", "/api/v1/app/SalesPriceBoard/price-board", "Product.Read", "legacy products list — separate", "Existing-other-domain"),
    ("29", "GET", "/api/v1/admin/Announcements", "Announcement.Read", "panel", "Implemented"),
    ("29", "POST", "/api/v1/admin/Announcements", "Announcement.Create", "panel Director", "Implemented"),
    ("29", "PUT", "/api/v1/admin/Announcements/{id}", "Announcement.Update", "panel", "Implemented"),
    ("29", "DELETE", "/api/v1/admin/Announcements/{id}", "Announcement.Delete", "panel", "Implemented"),
    ("29", "POST", "/api/v1/admin/Announcements/upload-image", "Announcement.Create", "panel", "Implemented"),
    ("29", "GET", "/api/v1/app/Announcements/{appKey}", "Announcement.AppRead", "delegate_app", "Implemented"),
    ("29", "GET", "/api/v1/app/Announcements/media/{file}", "AppRead|Read", "apps/panel preview", "Implemented"),
]:
    ws.append(r)

ws2 = wb.create_sheet("Permissions")
ws2.append(["Permission", "Templates", "Notes"])
for r in [
    ("PriceBoard.Read/Create/Update/Delete", "Monitor write; MainAccountant/BranchAccountant Read; Admin all", "OutsideAll"),
    ("Announcement.Read/Create/Update/Delete", "Director write; Admin all", "OutsideAll; not AppNote"),
    ("Announcement.AppRead", "Mandob + Motaba", "Server filters by appKey"),
]:
    ws2.append(r)

ws3 = wb.create_sheet("Audience")
ws3.append(["Feature", "Audience model", "Notification"])
ws3.append(["PriceBoard", "ProvinceId matching user branch provinces", "None"])
ws3.append(["Announcements", "AppKey delegate|follower + IsEnabled", "None — pull only"])

ws4 = wb.create_sheet("Tests")
ws4.append(["Suite", "Result", "Verdict"])
ws4.append(["PriceBoard+Announcement domain/permissions", "see unit-results", "Unit-tested"])
ws4.append(["EF migration PriceBoardAndAnnouncements on iso", "applied if CS present", "Schema iso"])
ws4.append(["API-live authenticated", "-", "Missing"])
ws4.append(["Browser-live", "-", "Missing"])
ws4.append(["Device-verified", "code wired; device run Missing", "Missing"])

wb.save(root / "Unified_Admin_Delivery_28_29.xlsx")

doc = Document()
doc.add_heading("Unified Admin Delivery 28-29 Architecture", 0)
doc.add_paragraph(
    "28 Price Board = province-scoped informational price ads (decimal amounts). "
    "Does not mutate Product or Order prices. Separate from SalesPriceBoard Products endpoint. "
    "29 Announcements = app-targeted home banner content (text XOR images). "
    "Not AppNote, not Notification/Push. Delivery is pull."
)
doc.add_heading("File responsibilities", 1)
for t in [
    "Domain: PriceBoardEntry; Announcement + AppKey/ContentType enums",
    "Application: PriceBoard CQRS; Announcements CQRS + image upload via IFileManager",
    "Api: admin/PriceBoard; app/PriceBoardAds; admin/Announcements; app/Announcements",
    "Panel: PriceBoardPage; AnnouncementsPage under oversight",
    "Apps: sales_employee_app PriceBoardPage -> PriceBoardAds; delegate_app home carousel -> Announcements/delegate",
]:
    doc.add_paragraph(t, style="List Bullet")
doc.add_heading("Open gaps", 1)
doc.add_paragraph(
    "10-27 gaps remain. 28/29 API-live and Browser-live Missing. "
    "Follower Flutter app absent in new repo (API contract ready). No production publish. Batch 30 not started."
)
doc.save(root / "Unified_Admin_Delivery_28_29_Architecture.docx")
print("ok")
