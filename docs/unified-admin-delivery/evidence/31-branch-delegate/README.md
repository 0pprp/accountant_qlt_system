# Evidence — Batch 31 Branch/Delegate

Database: `qalaat_22_23_iso` (localhost) — not production  
API: `http://127.0.0.1:15645`  
Panel: `http://127.0.0.1:8080` (VITE_API_BASE_URL → 15645)

## Files

| File | Content |
|---|---|
| `live-api.json` | دورة API-live (زيارة/NewSales/زبائن/WrongSale/تقييمات) |
| `live-api-supplement.json` | payments sum، mark-read، attachments auth، trend known |
| `live-api-31.ps1` | سكربت الإثبات |
| `unit-tests.txt` | 16 passed BranchDelegate+WrongSale |
| `vue-tsc.txt` / `vite-build.txt` | type-check 0 + build OK |
| `schema-iso.txt` | هجرات |
| `iso_seed_live/` | بذرة BM + Sold + payment للاختبار الحي |
| `iso_query/` | استعلام مستخدمين/قوائم |

## Results summary

- Visit Open idempotent / AlreadyOpen / BranchScope: PASS  
- Members snapshot + AssignedTasks live: PASS  
- Depart + PNG / reject exe / re-depart: PASS  
- Attachment download admin 200 / follower 403: PASS  
- Mandob first-collection-eval → mailbox → read → review → conflict: PASS  
- BranchCustomers search (name/phone/id + mandob/sell/rating): PASS — mandob=مندوب تحقق rating=ممتاز  
- BranchPayments rows+sum (list/date): PASS — count=1 total=25000  
- ManagementRatings trend endValue=10 ممتاز: PASS  
- NewSales unread 1→0 after mark-read; Sold unchanged: PASS  
- Browser admin customers/payments/sales/wrong-sales/ratings @ 360/768/1440: PASS  
- Flutter Device-verified: **Missing** (no device this session)

## Follow-up from SalesHaider extract

Source review confirmed: customers list fields (mandob/sell/rating), payments **rows** not sum-only, NewSales TOP 200 hard ceiling, visit attachments optional with 20MiB/file and no explicit file-count (request 25MiB). Applied in code; attachment MaxFilesPerDepart kept as memory guard=40 only.

## Build

```
dotnet test --filter FullyQualifiedName~BranchDelegate|FullyQualifiedName~WrongSale
→ Passed: 16

npx vue-tsc --noEmit → exit 0
npx vite build → ✓ built in 8.59s
```
