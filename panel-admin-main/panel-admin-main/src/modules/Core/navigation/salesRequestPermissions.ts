import { panelTabRef, SALES_REQUEST_PANEL_TAB_KEYS } from './panelTabAccess'

/**
 * صلاحيات تبويبات طلبات البيع — Panel.Tab.sr.* أولًا،
 * مع الإبقاء على مفاتيح SalesRequest القديمة للتوافق مع التوسيع في JWT.
 */
export const SALES_REQUEST_TAB_PERMISSIONS = [
  ...SALES_REQUEST_PANEL_TAB_KEYS.map((tabKey) => panelTabRef(tabKey)),
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

