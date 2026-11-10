export const SalesEmployeesEndpoints = {
  list: '/admin/SalesEmployees',
  ratingSummary: (id: number) => `/admin/SalesEmployees/${id}/rating-summary`,
} as const
