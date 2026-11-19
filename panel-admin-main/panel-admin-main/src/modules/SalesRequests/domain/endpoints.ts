/** مسارات API لطلبات البيع — لا تُضمَّن كسلاسل داخل المكوّنات. */
export const SalesRequestEndpoints = {
  list: '/admin/SalesRequests',
  counts: '/admin/SalesRequests/counts',
  unassigned: '/admin/SalesRequests/unassigned',
  byId: (id: number) => `/admin/SalesRequests/${id}`,
  timeline: (id: number) => `/admin/SalesRequests/${id}/timeline`,
  assignableEmployees: '/admin/SalesRequests/assignable-employees',
  assign: (id: number) => `/admin/SalesRequests/${id}/assign`,
  update: (id: number) => `/admin/SalesRequests/${id}`,
  reject: (id: number) => `/admin/SalesRequests/${id}/reject`,
  requestException: (id: number) =>
    `/admin/SalesRequests/${id}/request-exception`,
  approveException: (id: number) =>
    `/admin/SalesRequests/${id}/approve-exception`,
  rejectException: (id: number) =>
    `/admin/SalesRequests/${id}/reject-exception`,
  softDelete: (id: number) => `/admin/SalesRequests/${id}/soft-delete`,
  restore: (id: number) => `/admin/SalesRequests/${id}/restore`,
  matchSummary: '/admin/SalesRequests/match-summary',
  matchHits: (id: number) => `/admin/SalesRequests/${id}/match-hits`,
  duplicateRequests: (id: number) =>
    `/admin/SalesRequests/${id}/duplicate-requests`,
  assignBatch: '/admin/SalesRequests/assign-batch',
  returnToEmployee: (id: number) => `/admin/SalesRequests/${id}/return`,
  managerPrepare: (id: number) => `/admin/SalesRequests/${id}/prepare`,
  managerPend: (id: number) => `/admin/SalesRequests/${id}/pend`,
  cancelToUnassigned: (id: number) => `/admin/SalesRequests/${id}/cancel-to-unassigned`,
  transferBranch: (id: number) => `/admin/SalesRequests/${id}/transfer-branch`,
  deleteSoldImpact: (id: number) => `/admin/SalesRequests/${id}/delete-sold-impact`,
  deleteSold: (id: number) => `/admin/SalesRequests/${id}/delete-sold`,
  deletedSales: '/admin/SalesRequests/deleted-sales',
} as const
