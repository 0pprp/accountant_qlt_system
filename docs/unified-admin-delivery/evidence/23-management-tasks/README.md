# Evidence 23 — Management Tasks

- DB isolated: `qalaat_22_23_iso` (not production)
- Unit: `ManagementTaskDomain*` + `ManagementTaskInboxRulesTests` PASS
- Screens: إدارة المهام (canonical) ≠ مهامي مفوض ≠ مهامي فرع (`inboxOnly`)
- Confirm-after-transfer + self-complete without confirm: Domain enforced
- Attachments: ReadAttachment + CanSeeTask
- API-live: see `live-api.json`
- Panel realtime: `ManagementTasks/realtime/managementTaskRealtime.ts` wired on page (follow-up after audit)
- Audit follow-up: inbox gate already `[Authorize]` + handler; App Transfer/Confirm already present; Director includes ReadAttachment; remaining optional: due-time UI, cross-branch list without selected branch
