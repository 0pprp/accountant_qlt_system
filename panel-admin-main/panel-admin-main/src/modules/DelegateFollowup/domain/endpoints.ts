export const DelegateFollowupEndpoints = {
  lists: '/admin/DelegateFollowup/lists',
  summary: (listId: number) => `/admin/DelegateFollowup/lists/${listId}/summary`,
  customers: (listId: number) =>
    `/admin/DelegateFollowup/lists/${listId}/customers`,
  customerPayments: (listId: number, customerId: number) =>
    `/admin/DelegateFollowup/lists/${listId}/customers/${customerId}/payments`,
  boardRating: (listId: number) =>
    `/admin/DelegateFollowup/lists/${listId}/board-operator-rating`,
} as const
