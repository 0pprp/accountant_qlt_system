# SalesHaider FE_Company → panel.qalataldhaman.com page map

Excluded: المتابع الرئيسي, موظف الفلترة.

## المدير / المراقب

| Reference page | Panel route |
|---|---|
| director-ratings.vue | /management/oversight/ratings |
| director-summary.vue | /management/oversight/summary |
| director-delegate-followup.vue | /management/director/delegate-followup |
| director-delegate-followup-customers.vue | /management/director/delegate-followup/customers |
| director-sales-followup.vue | /management/branch/sale-followup (also sales followup) |
| director-follower-work.vue (+tabs) | /management/oversight/follower-work/* |
| sales-manager-followers-map.vue | /management/oversight/follower-work/live-map |
| sales-manager-followers-routes.vue | /management/oversight/follower-work/routes |
| director-committee-tasks.vue | /management/oversight/committee-tasks |
| director-notes.vue | /management/oversight/app-notes |
| director-waivers.vue | /management/oversight/waivers |
| price-board.vue | /management/oversight/price-board |
| director-announcements.vue | /management/oversight/announcements |
| director-tutorial-videos.vue | /management/oversight/tutorials |
| director-assigned-tasks.vue | /management/tasks → delegate/branch assigned |
| director-branch-visits.vue | /management/director or delegate branch-visits |

## مدير مفوض (dm-*)

| Reference | Panel |
|---|---|
| dm-dashboard.vue | /management/delegate/home |
| dm-assigned-tasks.vue | /management/delegate/assigned-tasks |
| dm-complaints.vue | /management/delegate/complaints |
| dm-exceptions.vue | /management/delegate/exceptions |
| dm-wrong-sales.vue | /management/delegate/wrong-sales |
| dm-waivers.vue | /management/delegate/waivers |
| dm-branch-manager-users.vue | /management/delegate/accounts |
| dm-branch-visits.vue | /management/delegate/branch-visits |
| dm-settings.vue | /management/delegate/settings |
| dm-stats.vue / dm-customer-profile.vue | partial / customer audit profile |

## مدير مبيعات

| Reference | Panel |
|---|---|
| sales-manager-dashboard.vue | /sale/overview |
| sales-manager-requests.vue | /sale/requests |
| sales-manager-employees.vue | /sale/employees |
| sales-manager-excel-search.vue | /sale/excel-search |
| sales-manager-followers*.vue | follower-work + /sale/live-map,/sale/routes |
| sales-manager-sales-summary.vue | /sale/summary |
| sales-manager-legal-followups.vue | /sale/legal |
| sales-manager-ratings.vue | management ratings / sales ratings |

## مدير فرع

| Reference | Panel |
|---|---|
| customers-list / declining | /management/branch/customers, declining |
| decision-board / decisions-list | /management/branch/decision-history |
| legal names flows | /management/branch/legal-names |
| payments-* | /management/branch/payments |
| sales / wrong sales | /management/branch/sales, wrong-sales |
| waivers (branch) | /management/branch/waivers |
| assigned tasks | /management/branch/assigned-tasks |
| visit workspace | /management/branch/visit-workspace |

## محاسب رئيسي

| Reference | Panel |
|---|---|
| accountant-ratings.vue | ratings modules |
| accountant-wrong-sales.vue | wrong sales (accountant audience if present) |
| accountant-legal-search.vue | legal / customer decisions |
| waiver-accountant-requests.vue | waivers apply stage |
| users-list.vue | /management/.../users or user list |
| user-active.vue | user management |
| backup-database.vue | not in unified panel (gap) |

## Notes

- Monitor vs director differ by allowed paths in reference router; panel uses permission modules instead of UserType string.
- backup-database and some accountant-only Vuexy utilities may have no 1:1 panel page.
