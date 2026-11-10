# Harden بوابة زيارة الفرع (نطاق 08 فقط)

## السلوك

| مسار | زيارة مطابقة للفرع المحدد؟ | النتيجة |
|---|---|---|
| `/management/branch/*` أعمال | لا / loading / error | لا `RouterView` للأعمال → صفر طلبات أعمال |
| `/management/branch/visit-workspace` | أي | تُركَّب دائمًا — بلا redirect loop |
| `/management/oversight/*` | — | مستثنى (لا بوابة) |
| `/management/delegate/*` inkl. سجل الزيارات | — | مستثنى (تقارير المفوض) |

- نقص `BranchVisit.Read` ≠ تجاوز؛ الفحص يحتاج Read أو Open فقط لاستعلام `/active`.
- السيرفر: `BranchVisit.VisitRequired` / `VisitBranchMismatch` عبر `BranchVisitRequirement` على أكشنات أعمال الفرع.

## إثبات

```
panel: npm test -- branchVisitGate.component.test.ts branchVisitGate.logic.test.ts
  → 12 passed (Router + axios adapter + ManagementWorkspace)
api:   dotnet test --filter BranchDelegate.Batch31
  → VisitRequired بدون زيارة؛ زيارة A لا تسرّب نطاق B
```

لا تغيير تصميم واجهة. لا إنتاج. لا 09.
