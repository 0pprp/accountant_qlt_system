# Authz matrix 32 (iso accounts only)

| Account | Role | Branches | Notes |
|---|---|---|---|
| admin | Admin | 1,2 | Create/Decide/Apply waivers (not own create for Decide) |
| follower_iso | Motaba | 1 | 403 Waivers; AppNotes create OK |
| mandob_iso | Mandob | 1 | AppNotes create OK |
| bm_iso | BranchManager | 1 | Waiver create OK; no ManagePermissions assumed from 31 |

Missing dedicated accounts on iso: delegate manager, sales employee, noperm.
Server deny proven: 401 no token; 403 follower waivers; SelfDecide on own waiver.
