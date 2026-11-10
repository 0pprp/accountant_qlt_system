export const SalesLegalEndpoints = {
  list: '/admin/LegalFollowups',
  byId: (id: number) => `/admin/LegalFollowups/${id}`,
  followers: '/admin/LegalFollowups/followers',
  create: '/admin/LegalFollowups',
  cancel: (id: number) => `/admin/LegalFollowups/${id}/cancel`,
  documentFile: (caseId: number, docId: number) =>
    `/admin/LegalFollowups/${caseId}/documents/${docId}/file`,
} as const
