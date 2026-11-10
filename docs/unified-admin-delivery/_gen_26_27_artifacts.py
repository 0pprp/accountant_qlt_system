from openpyxl import Workbook
from docx import Document
from pathlib import Path

root = Path(__file__).resolve().parent
ev26 = root / "evidence" / "26-waivers"
ev27 = root / "evidence" / "27-legal-decisions"
ev26.mkdir(parents=True, exist_ok=True)
ev27.mkdir(parents=True, exist_ok=True)

wb = Workbook()
ws = wb.active
ws.title = "Endpoints"
ws.append(["Area", "Method", "Path", "Permission", "Consumer", "Status"])
for r in [
    ("26 Waiver", "GET", "/api/v1/admin/Waivers", "Waiver.Read", "oversight/branch/delegate", "Implemented"),
    ("26 Waiver", "GET", "/api/v1/admin/Waivers/{id}", "Waiver.Read", "panels detail", "Implemented"),
    ("26 Waiver", "POST", "/api/v1/admin/Waivers", "Waiver.Create", "branch create", "Implemented"),
    ("26 Waiver", "POST", "/api/v1/admin/Waivers/{id}/approve", "Waiver.Decide", "delegate", "Implemented"),
    ("26 Waiver", "POST", "/api/v1/admin/Waivers/{id}/reject", "Waiver.Decide", "delegate", "Implemented"),
    ("26 Waiver", "POST", "/api/v1/admin/Waivers/{id}/apply", "Waiver.Apply", "accountant", "Implemented"),
    ("27 LegalNames", "GET", "/api/v1/admin/CustomerDecisions/legal-names", "LegalNames.Read", "branch", "Implemented"),
    ("27 Decisions", "GET", "/api/v1/admin/CustomerDecisions", "CustomerDecision.Read", "branch history", "Implemented"),
    ("27 Decisions", "POST", "/api/v1/admin/CustomerDecisions", "CustomerDecision.Create", "branch dialog", "Implemented"),
    ("27 Conversion", "POST", "/api/v1/admin/CustomerDecisions/legal-conversions/{id}/approve", "LegalConversion.Decide", "monitor/director", "Implemented"),
    ("27 Conversion", "POST", "/api/v1/admin/CustomerDecisions/legal-conversions/{id}/reject", "LegalConversion.Decide", "monitor/director", "Implemented"),
]:
    ws.append(r)

ws2 = wb.create_sheet("Permissions")
ws2.append(["Permission", "Role templates", "Notes"])
for r in [
    ("Waiver.Read", "Director,Monitor,BranchManager,Accountants,Admin", "OutsideAll"),
    ("Waiver.Create", "BranchManager,Admin", "Not Decide/Apply"),
    ("Waiver.Decide", "Director,Admin", "Self-decide blocked"),
    ("Waiver.Apply", "Branch/Main/Chief Accountant,Admin", "SellAmount discount only"),
    ("LegalNames.Read", "BranchManager,Admin", "Weak payer queue"),
    ("CustomerDecision.Read", "BranchManager,Director,Monitor,Accountants,Admin", "Not ActivityLog"),
    ("CustomerDecision.Create", "BranchManager,Admin", "Continue/Fake/LegalRequest/WaiverRequest"),
    ("LegalConversion.Decide", "Director,Monitor,ChiefAccountant,Admin", "Sets IsLegal"),
]:
    ws2.append(r)

ws3 = wb.create_sheet("States")
ws3.append(["Entity", "From", "Action", "Actor", "To", "Side effects"])
for r in [
    ("Waiver", "-", "Create", "Branch", "Pending", "Request row"),
    ("Waiver", "Pending", "Reject", "Delegate", "Rejected", "No finance"),
    ("Waiver", "Pending", "Approve", "Delegate", "AwaitingAccountant", "Decision WaiverRequest"),
    ("Waiver", "AwaitingAccountant", "Apply", "Accountant", "Approved", "Cap SellAmount; Decision Waiver; idempotent"),
    ("LegalConversion", "-", "LegalRequest", "Branch", "Pending", "Decision LegalRequest"),
    ("LegalConversion", "Pending", "Approve", "Monitor/Director", "Approved", "IsLegal=true"),
    ("LegalConversion", "Pending", "Reject", "Monitor/Director", "Rejected", "Reason required"),
    ("CustomerDecision", "-", "Continue", "Branch", "ledger", "Snooze +7d"),
    ("CustomerDecision", "-", "FakeSale", "Branch", "ledger", "IsFakeSale=true"),
]:
    ws3.append(r)

ws4 = wb.create_sheet("Tests")
ws4.append(["Suite", "Count", "Evidence", "Verdict"])
ws4.append(["Application.Tests Waivers+CustomerDecisions", "29 passed", "evidence/26-waivers/unit-results.txt", "Unit-tested"])
ws4.append(["EF migration on qalaat_22_23_iso", "Done", "evidence/26-waivers/migration-iso.txt", "Schema iso"])
ws4.append(["Authenticated API-live cycle", "-", "-", "Missing"])
ws4.append(["Browser-live", "-", "-", "Missing"])
ws4.append(["Device Flutter apps", "Absent in new repo", "27 matrix", "Missing — API contracts ready"])

ws5 = wb.create_sheet("Fields")
ws5.append(["Screen", "Key fields / counters"])
ws5.append(["Admin Waivers", "status, pendingCount, awaitingAccountantCount, approvedAmount, requestNote, expectedVersion"])
ws5.append(["Branch Legal Names", "saleTotal, weekPaid, remaining, paidPercent, daysSinceLastPayment"])
ws5.append(["Branch Decision History", "decisionTypeLabel, actorName, note, snoozeUntil, customerIsLegal, customerIsFakeSale"])

wb.save(root / "Unified_Admin_Delivery_26_27.xlsx")

doc = Document()
doc.add_heading("Unified Admin Delivery 26-27 Architecture", 0)
doc.add_paragraph(
    "Batches 26 (Waivers) and 27 (Legal names / branch decisions / decision ledger) "
    "share a proven domain split: one Waiver request lifecycle; CustomerDecision as "
    "functional ledger; LegalConversion for IsLegal; LegalFollowupCases remain sales-only."
)
doc.add_heading("Domain boundaries", 1)
for t in [
    "CustomerWaiverRequest: Pending -> Rejected | AwaitingAccountant -> Approved. Financial effect only on Apply: reduce Order.SellAmount by Cap(sale-paid, approved).",
    "CustomerDecision: append-only business decisions. Not ActivityLog.",
    "LegalConversionRequest: Pending -> Approved|Rejected; Approve sets Customer.IsLegal.",
    "WeakPayerRules define Legal Names queue (<=3.8% week paid, >=40 days, exclude legal/fake/snooze/open waiver/pending conversion).",
]:
    doc.add_paragraph(t, style="List Bullet")
doc.add_heading("File responsibilities", 1)
for t in [
    "Domain: WaiverAggregate, CustomerDecisionAggregate, LegalConversionAggregate",
    "Application: Waivers CQRS; CustomerDecisions commands/queries; WeakPayerRules",
    "Api: WaiversController; CustomerDecisionsController",
    "Panel: WaiversPage (oversight/branch/delegate scopes); LegalNamesPage; CustomerDecisionsPage",
    "Seeds: Permissions OutsideAll; RolePermissions / RoleDefaultMatrix templates",
]:
    doc.add_paragraph(t, style="List Bullet")
doc.add_heading("Integrations", 1)
doc.add_paragraph(
    "Admin oversight tab Waivers; branch workspace Legal Names / Branch Waivers / "
    "Decision History; delegate workspace Waivers inbox. Flutter branch/delegate apps "
    "absent in new repo — panel tabs + HTTP contracts are the consumers."
)
doc.add_heading("Open gaps", 1)
doc.add_paragraph(
    "22-25 gaps remain open. 26/27: Unit-tested yes; schema migrated on local iso "
    "qalaat_22_23_iso; authenticated API-live cycle and Browser-live Missing; no "
    "production publish; batch 28 not started."
)
doc.save(root / "Unified_Admin_Delivery_26_27_Architecture.docx")
print("xlsx+docx written")
