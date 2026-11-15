# 34 — خريطة المبيعات

- DB isolated: `qalaat_22_23_iso` (not production)
- API-live: **PASS** — provinces/lists/points/detail على http://127.0.0.1:4200 (انظر `api-live.json`)
- Browser-live: **PASS** — `ui-provinces.png` / `ui-map-cluster.png` / `ui-detail-sheet.png`
- Unit: SalesMapRulesTests (6) + navigation.registry (11)
- Docs: `ManagementSalesMap_API.xlsx` + `ManagementSalesMap_Architecture.docx`
- Fixes this session: seed `ManagementSalesMap.OutsideAll` في DatabaseInitializer؛ تطبيع `listIds` الفارغ = كل القوائم
