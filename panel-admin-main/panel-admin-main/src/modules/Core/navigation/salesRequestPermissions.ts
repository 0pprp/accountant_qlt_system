/** صلاحيات تبويبات طلبات البيع — مشتركة بين المبيعات ولوحة الإدارة */
export const SALES_REQUEST_TAB_PERMISSIONS = [
  { module: 'SalesRequest' as const, action: 'ReadUnassigned' as const },
  { module: 'SalesRequest' as const, action: 'TabIncoming' as const },
  { module: 'SalesRequest' as const, action: 'TabPrepared' as const },
  { module: 'SalesRequest' as const, action: 'TabInspected' as const },
  { module: 'SalesRequest' as const, action: 'TabPending' as const },
  { module: 'SalesRequest' as const, action: 'TabRejected' as const },
  { module: 'SalesRequest' as const, action: 'TabSold' as const },
  { module: 'SalesRequest' as const, action: 'TabSent' as const },
  { module: 'SalesRequest' as const, action: 'TabExceptions' as const },
  { module: 'SalesRequest' as const, action: 'TabDeleted' as const },
] as const

