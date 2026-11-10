export const SalesFollowupEndpoints = {
  employees: '/admin/SalesFollowup/employees',
  detail: (id: number) => `/admin/SalesFollowup/employees/${id}`,
  appBlock: (id: number) => `/admin/SalesFollowup/employees/${id}/app-block`,
  managerRating: (id: number) =>
    `/admin/SalesFollowup/employees/${id}/manager-rating`,
} as const
