# -*- coding: utf-8 -*-
from pathlib import Path
from openpyxl import Workbook
from openpyxl.styles import Alignment, Border, Font, PatternFill, Side
from docx import Document

ROOT = Path(__file__).resolve().parent
HEADER_FILL = PatternFill("solid", fgColor="006A64")
HEADER_FONT = Font(color="FFFFFF", bold=True, name="Arial")
BODY_FONT = Font(name="Arial", size=11)
THIN = Border(
    left=Side(style="thin"), right=Side(style="thin"),
    top=Side(style="thin"), bottom=Side(style="thin"),
)

def style_header(ws, cols):
    for i, h in enumerate(cols, 1):
        c = ws.cell(1, i, h)
        c.fill, c.font, c.border = HEADER_FILL, HEADER_FONT, THIN
        c.alignment = Alignment(wrap_text=True, vertical="center")

def append_rows(ws, rows):
    for r in rows:
        ws.append(list(r))
        for cell in ws[ws.max_row]:
            cell.font, cell.border = BODY_FONT, THIN
            cell.alignment = Alignment(wrap_text=True, vertical="top")

wb = Workbook()
ws = wb.active
ws.title = "Identity"
style_header(ws, ["OldConcept","OldKey","NewEntity","Relation","Evidence","Gap"])
append_rows(ws, [
    ("Follower card","Users.UserType=متابع","User via OrderList.MotabaId","Current assignment","FollowerDirectory+FollowerUserLists vs MotabaId","No historical assignment ledger"),
    ("List ACL","FollowerUserLists","OrderList.MotabaId","Current only","Coverage_02","—"),
    ("Branch scope","City/lists fan-out","OrderList.BranchId ∩ UserBranches","Via assigned lists","—","Not all branch lists to every Motaba"),
    ("Committee officer","Separate old app path","Same Motaba role (product label)","Same account","committee_officer AGENTS.md","Naming ≠ second entity"),
    ("Online/shift","HasActiveShift≈0","Omitted","No trusted source","Directory stub","No login-as-activity"),
    ("Delegate/Seller","Mandob/Seller","MandobId/SellerId","Distinct","—","No merge"),
])

ws2 = wb.create_sheet("CardFields")
style_header(ws2, ["Field","Source","Permission","Status05"])
append_rows(ws2, [
    ("followerId/name","Motaba user","directory","Implemented"),
    ("branches[]","Assigned lists","directory","Implemented"),
    ("assignedListCount","Count MotabaId lists","directory","Implemented"),
    ("phantomRating* / trends","SalesRatings ActiveRatingCohort phantom","FollowerWork.Read only","Implemented"),
    ("section counters×8","summaries/work bundle","section perms","Not sent — not_implemented"),
    ("online/lastLogin","—","—","Forbidden/omitted"),
])

ws3 = wb.create_sheet("Endpoints")
style_header(ws3, ["Method","Path","Auth","Notes","Status"])
append_rows(ws3, [
    ("GET","/api/v1/admin/FollowerWork/followers","FollowerWork.Read OR section Read","branchId/search/page/pageSize/sort allowlist; no section bundle","Implemented"),
])

ws4 = wb.create_sheet("Permissions")
style_header(ws4, ["Permission","Seed Director/Monitor","Opens directory","Marks section ready","Notes"])
append_rows(ws4, [
    ("FollowerWork.Read","yes","yes","shell only","Card metrics require this"),
    ("FollowerSalesReview.Read","no","yes","no","Batch 06"),
    ("FollowerDeclining.Read","no","yes","no","Batch 07"),
    ("FollowerNotes.Read","no","yes","no","Batch 08"),
    ("FollowerOldCustomers.Read","no","yes","no","Batch 09"),
    ("FollowerDelegateNotes.Read","no","yes","no","Batch 10"),
    ("FollowerCollection.Read","no","yes","no","Batch 11"),
    ("FollowerLocation.Read","no","yes","no","Batch 12"),
    ("FollowerRoute.Read","no","yes","no","Batch 13"),
])

ws5 = wb.create_sheet("Sections")
style_header(ws5, ["Section","Path","Status","Evidence"])
append_rows(ws5, [
    ("Shell/directory","/management/follower-work","ready","Coverage_05"),
    ("Sales auth",".../sales","not_implemented","06"),
    ("Declining",".../declining","not_implemented","07"),
    ("Notes",".../notes","not_implemented","08"),
    ("Old customers",".../old-customers","not_implemented","09"),
    ("Delegate notes",".../delegate-notes","not_implemented","10"),
    ("Collection",".../collection","not_implemented","11"),
    ("Live map",".../live-map","not_implemented","12"),
    ("Routes",".../routes","not_implemented","13"),
])

ws6 = wb.create_sheet("Files")
style_header(ws6, ["Layer","Path"])
append_rows(ws6, [
    ("App","Application/FollowerWork/**"),
    ("Api","FollowerWorkController.cs"),
    ("Panel","modules/FollowerWork/**"),
    ("Nav","navigation.registry + Management/sections"),
    ("Docs","05-follower-identity-matrix.md + 05-follower-card-field-matrix.md + Coverage_05"),
])

wb.save(ROOT / "Unified_Admin_Delivery_05_FollowerWork.xlsx")

doc = Document()
doc.add_heading("05 — هيكل عمل المتابعين والبطاقات", level=1)
doc.add_paragraph(
    "دفعة الهيكل فقط: تنقل الأب، هوية المتابع عبر OrderList.MotabaId، "
    "عقد بطاقات GET followers، صلاحيات الدليل والأقسام المسجّلة. "
    "لا تنفيذ للأقسام الثمانية. ليس ManagementTasks ولا SalesFollowup."
)
doc.add_heading("الملفات والمسؤوليات", level=2)
doc.add_paragraph(
    "FollowerWorkDataService: تجميع التكليف + دفعة مقاييس التقييم للقوائم المعروضة. "
    "FollowerWorkAccess: OR بين FollowerWork.Read وصلاحيات الأقسام للدليل؛ "
    "المقاييس على البطاقة تتطلب الأب فقط. "
    "الواجهة: FollowerWorkPage + context URL (branchId/followerId/search/page/sort) "
    "مع إبطال اختيار المتابع عند تغيير المحافظة."
)
doc.add_heading("التفويض والكاش", level=2)
doc.add_paragraph(
    "فرع غير مخول يُرفض. علاقة متابع↔فرع عبر قائمة مسندة. "
    "Query keys تشمل sessionGeneration وbranchGeneration. "
    "لا prefetch لأقسام not_implemented. لا نظام session موازٍ."
)
doc.add_heading("استخدام الأقسام القادمة", level=2)
doc.add_paragraph(
    "الأقسام 06–13 تستخدم نفس السياق (followerId/branchId) وuseFollowerReturnContext. "
    "تفعيل route القسم وصلاحية القسم في دفعتها؛ لا تعلن ready قبل التنفيذ."
)
doc.add_heading("متبقي", level=2)
doc.add_paragraph("لقطات 360/768/1440 على قاعدة معزولة؛ تحقق حي للعلاقات؛ دفعة 06.")
doc.save(ROOT / "Unified_Admin_Delivery_05_Architecture.docx")
print("Wrote 05 excel+docx")
