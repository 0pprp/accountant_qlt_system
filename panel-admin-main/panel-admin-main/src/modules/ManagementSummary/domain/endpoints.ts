export const ManagementSummaryEndpoints = {
  matrix: '/admin/ManagementSummary/matrix',
  details: '/admin/ManagementSummary/details',
} as const

export const METRIC_OPTIONS: Array<{
  value: 'receipts' | 'expenses' | 'salePrice' | 'purchasePrice' | 'buys'
  title: string
}> = [
  { value: 'receipts', title: 'الواردات' },
  { value: 'expenses', title: 'الصرفيات' },
  { value: 'salePrice', title: 'سعر البيع' },
  { value: 'purchasePrice', title: 'سعر الشراء' },
  { value: 'buys', title: 'المشتريات' },
]
