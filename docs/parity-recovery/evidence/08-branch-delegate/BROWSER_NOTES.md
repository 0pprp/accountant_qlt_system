Browser-live 08 (bm_iso):
- /management/branch → redirected to /management/branch/visit-workspace (no active visit for actor)
- Independent parents visible: إدارة الفروع + المدير المفوض
- Shots: branch-visit-360.png, branch-visit-768.png, branch-visit-1440.png

Visit-gate harden (component Router+API, no redesign):
- Delayed /active → zero BranchNewSales/Customers/Payments/Declining while loading; then redirect visit-workspace; no loop on visit page
- Open visit for branch B + selected A → ops not mounted
- Network fail / missing BranchVisit.Read → ops blocked (Read is not a bypass)
- Matching open visit → ops mount + list request; branch switch clears ops
- Server: BranchVisitRequirement on branch ops (UI redirect alone insufficient)
- See VISIT_GATE_HARDEN.md
