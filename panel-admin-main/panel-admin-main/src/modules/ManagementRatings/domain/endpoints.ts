export const ManagementRatingsEndpoints = {
  branches: '/admin/ManagementRatings/branches',
  catalog: '/admin/ManagementRatings/catalog',
  periodSummary: '/admin/ManagementRatings/period-summary',
  trend: '/admin/ManagementRatings/trend',
  customers: '/admin/ManagementRatings/customers',
  listPaymentsSum: '/admin/ManagementRatings/list-payments-sum',
} as const

export const RATING_BUCKETS = [
  { key: 'excellent', label: 'ممتاز', countKey: 'excellentCount' as const },
  { key: 'good', label: 'جيد', countKey: 'goodCount' as const },
  { key: 'weak', label: 'ضعيف', countKey: 'weakCount' as const },
  { key: 'rejected', label: 'متلكئ', countKey: 'laggingCount' as const },
  { key: 'bad', label: 'سيء', countKey: 'badCount' as const },
] as const
