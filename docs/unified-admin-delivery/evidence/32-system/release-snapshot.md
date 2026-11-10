# Release snapshot — Batch 32 candidate

**Captured:** 2026-11-09T19:15Z (local)  
**Branch:** `development`  
**HEAD commit (last committed):** `650e3a16f66668378c3273d74c9cb7198fdc8e4d` — *Add committee pending-sale authenticate endpoint for Motaba.*  
**Working tree:** dirty — ~130 modified tracked + ~260 untracked entries (batches 10–31 work not fully committed).  
**Release identity:** **working-tree candidate** (not a single clean commit). Prior live evidence remains valid only where code for that surface was not changed after the evidence timestamp; Batch 31 follow-up (BranchCustomers/BranchPayments DTO) **did** change after 31 Browser-live — re-probed via API.

## Toolchain

| Tool | Version |
|---|---|
| .NET SDK | 10.0.401 (target TFM net9.0) |
| Node | v22.18.0 |
| npm | 10.9.3 |
| Flutter | 3.47.1 stable |

## Migrations in scope (46)

Latest: `20261109184704_Branch31VisitMembersAttachmentsAndWrongSaleSource`  
Snapshot SHA256: `6738F9476F9B6F1215F8D64D6A7BCDB217A174EBB31E12487BD61A8EEB168239`  
`PendingModelChangesWarning` ignore gated by `PendingModelChangesGate` (local verify DB only; not production).

## Databases tested

| DB | Role | Migrations | Seed |
|---|---|---|---|
| `qalaat_22_23_iso` | upgrade / integration | 46 / latest Branch31… | users=4 permissions=195 |
| `qalaat_32_fresh` | empty → `dotnet ef database update` | 46 / same latest | users=0 until API start (seed on boot) |

## Units / apps in release candidate

| Surface | Path |
|---|---|
| API | `api-main/api-main` |
| Panel | `panel-admin-main/panel-admin-main` |
| Delegate app | `delegate_app` |
| Sales employee | `sales_employee_app` |
| Committee | `committee_officer_application` |
| Legacy team app | `app-dev/app-dev` |
| Branch/Delegate BM apps | **Web panel only** (no Flutter BM/DM) |

## Build fingerprints (Release)

| Artifact | SHA256 |
|---|---|
| `Api.dll` (publish) | `ED9A9C0CD542841ED77037BED14738DDB7E3E2354DBBD8403E0484291056C167` |
| `Application.dll` | `DC94CE412FA62B19CED7A63903AE14D0FDB122EC1A392963BAFF577021ABA767` |
| Panel vite index chunk | see `vite-build.txt` / dist after build |

## Build results

| Check | Result |
|---|---|
| `dotnet build Api -c Release` | 0 errors |
| `dotnet test Application.Tests -c Release` | **412 passed** (3 stale expectations fixed) |
| `vue-tsc --noEmit` | 0 |
| `vite build` | ✓ ~6.8s |
| `dotnet publish Api -c Release` | OK → `evidence/32-system/artifacts/api-publish` |
| Flutter analyze | Build-tested / infos — Device-verified Missing |

**No production deploy in this batch.**
