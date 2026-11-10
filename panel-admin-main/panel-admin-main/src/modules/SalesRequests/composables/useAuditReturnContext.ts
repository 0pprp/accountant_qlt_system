import type { LocationQueryRaw } from 'vue-router'
import type { MatchCriterion, SearchSource } from '../types'

export const AUDIT_RETURN_ROUTE = 'UnassignedSalesRequestsRoute'
export const AUDIT_RETURN_WORKSPACE = 'SalesRequestsWorkspaceRoute'
export const AUDIT_RETURN_SALES_FOLLOWUP = 'SalesFollowupRoute'
export const AUDIT_RETURN_DELEGATE_FOLLOWUP =
  'ManagementDirectorDelegateFollowupRoute'
export const AUDIT_RETURN_DELEGATE_FOLLOWUP_CUSTOMERS =
  'ManagementDirectorDelegateFollowupCustomersRoute'
export const AUDIT_RETURN_MANAGEMENT_RATINGS = 'ManagementRatingsRoute'
export const AUDIT_RETURN_BRANCH_RATINGS = 'ManagementBranchRatingsRoute'
export const AUDIT_RETURN_DELEGATE_RATINGS = 'ManagementDelegateRatingsRoute'
export const AUDIT_RETURN_FOLLOWER_WORK = 'ManagementFollowerWorkRoute'
export const AUDIT_RETURN_FOLLOWER_SALES = 'ManagementFollowerSalesRoute'
export const AUDIT_RETURN_FOLLOWER_DECLINING = 'ManagementFollowerDecliningRoute'
export const AUDIT_RETURN_FOLLOWER_NOTES = 'ManagementFollowerNotesRoute'
export const AUDIT_RETURN_FOLLOWER_OLD = 'ManagementFollowerOldCustomersRoute'
export const AUDIT_RETURN_FOLLOWER_DELEGATE_NOTES =
  'ManagementFollowerDelegateNotesRoute'
export const AUDIT_RETURN_FOLLOWER_COLLECTION =
  'ManagementFollowerCollectionRoute'
export const AUDIT_RETURN_FOLLOWER_LIVE_MAP = 'ManagementFollowerLiveMapRoute'
export const AUDIT_RETURN_FOLLOWER_ROUTES = 'ManagementFollowerRoutesRoute'
export const AUDIT_RETURN_SALE_COMPLETED = 'SaleCompletedRoute'
export const AUDIT_RETURN_SALE_ROUTES = 'SaleRoutesRoute'

const ALLOWED_RETURN_ROUTES = new Set([
  AUDIT_RETURN_ROUTE,
  AUDIT_RETURN_WORKSPACE,
  AUDIT_RETURN_SALES_FOLLOWUP,
  AUDIT_RETURN_DELEGATE_FOLLOWUP,
  AUDIT_RETURN_DELEGATE_FOLLOWUP_CUSTOMERS,
  AUDIT_RETURN_MANAGEMENT_RATINGS,
  AUDIT_RETURN_BRANCH_RATINGS,
  AUDIT_RETURN_DELEGATE_RATINGS,
  AUDIT_RETURN_FOLLOWER_WORK,
  AUDIT_RETURN_FOLLOWER_SALES,
  AUDIT_RETURN_FOLLOWER_DECLINING,
  AUDIT_RETURN_FOLLOWER_NOTES,
  AUDIT_RETURN_FOLLOWER_OLD,
  AUDIT_RETURN_FOLLOWER_DELEGATE_NOTES,
  AUDIT_RETURN_FOLLOWER_COLLECTION,
  AUDIT_RETURN_FOLLOWER_LIVE_MAP,
  AUDIT_RETURN_FOLLOWER_ROUTES,
  AUDIT_RETURN_SALE_COMPLETED,
  AUDIT_RETURN_SALE_ROUTES,
])

/** مسارات عمل المتابعين المسموحة كـ returnTo نصي (مسار نسبي). */
const ALLOWED_RETURN_PATH_PREFIXES = [
  '/management/follower-work',
  '/management/oversight/follower-work',
  '/management/director/follower-work',
  '/management/branch/customers',
] as const

export interface AuditReturnContext {
  branchId?: string | null
  provinceId?: string | null
  sourceType?: string | null
  fromDate?: string | null
  toDate?: string | null
  saleDateFrom?: string | null
  saleDateTo?: string | null
  sellerId?: string | null
  employeeId?: string | null
  search?: string | null
  page?: string | null
  sort?: string | null
  requestId?: string | null
  criterion?: MatchCriterion | string | null
  searchSource?: SearchSource | string | null
  scrollY?: string | null
}

/** وجهات رجوع داخلية معروفة فقط — اسم مسار Vue أو مسار نسبي آمن. */
export function isAllowedReturnRoute(name: unknown): boolean {
  if (typeof name !== 'string' || !name.trim()) return false
  if (ALLOWED_RETURN_ROUTES.has(name)) return true
  return isAllowedReturnPath(name)
}

export function isAllowedReturnPath(raw: unknown): boolean {
  if (typeof raw !== 'string') return false
  const value = raw.trim()
  if (!value.startsWith('/') || value.startsWith('//')) return false
  if (value.includes('://') || value.includes('\\')) return false
  const path = value.split('?')[0] ?? value
  return ALLOWED_RETURN_PATH_PREFIXES.some(
    (p) => path === p || path.startsWith(`${p}/`)
  )
}

export function buildAuditReturnQuery(
  ctx: AuditReturnContext
): LocationQueryRaw {
  const query: LocationQueryRaw = {}
  for (const [key, value] of Object.entries(ctx)) {
    if (value != null && value !== '') query[key] = String(value)
  }
  return query
}

export function parseAuditReturnQuery(
  query: Record<string, unknown>
): AuditReturnContext {
  return {
    branchId: str(query.branchId),
    provinceId: str(query.provinceId),
    sourceType: str(query.sourceType),
    fromDate: str(query.fromDate),
    toDate: str(query.toDate),
    saleDateFrom: str(query.saleDateFrom),
    saleDateTo: str(query.saleDateTo),
    sellerId: str(query.sellerId),
    employeeId: str(query.employeeId),
    search: str(query.search),
    page: str(query.page),
    sort: str(query.sort),
    requestId: str(query.requestId),
    criterion: str(query.criterion),
    searchSource: str(query.searchSource),
    scrollY: str(query.scrollY),
  }
}

function str(value: unknown): string | null {
  if (value == null) return null
  const s = Array.isArray(value) ? String(value[0] ?? '') : String(value)
  return s === '' ? null : s
}

export function restoreScroll(scrollY: string | null | undefined) {
  const y = Number(scrollY)
  if (!Number.isFinite(y) || y < 0) return
  requestAnimationFrame(() => {
    window.scrollTo({ top: y, behavior: 'auto' })
  })
}
