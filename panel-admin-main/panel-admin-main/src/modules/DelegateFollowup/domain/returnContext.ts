/** سياق عودة آمن من بروفايل/تفاصيل — allowlist فقط، بلا redirect تعسفي. */

export const DELEGATE_FOLLOWUP_BOARD_ROUTE =
  'ManagementDirectorDelegateFollowupRoute'
export const DELEGATE_FOLLOWUP_CUSTOMERS_ROUTE =
  'ManagementDirectorDelegateFollowupCustomersRoute'

const ALLOWED_RETURN_ROUTES = new Set([
  DELEGATE_FOLLOWUP_BOARD_ROUTE,
  DELEGATE_FOLLOWUP_CUSTOMERS_ROUTE,
])

export type DelegateFollowupReturnContext = {
  returnTo: string
  branchId: number
  listId: number
  filter?: string
  search?: string
  sort?: string
  page?: string
  scrollY?: string
}

export function buildSafeReturnQuery(
  ctx: DelegateFollowupReturnContext
): Record<string, string> {
  const q: Record<string, string> = {
    returnTo: ALLOWED_RETURN_ROUTES.has(ctx.returnTo)
      ? ctx.returnTo
      : DELEGATE_FOLLOWUP_CUSTOMERS_ROUTE,
    branchId: String(ctx.branchId),
    listId: String(ctx.listId),
  }
  if (ctx.filter) q.filter = ctx.filter
  if (ctx.search) q.search = ctx.search
  if (ctx.sort) q.sort = ctx.sort
  if (ctx.page) q.page = ctx.page
  if (ctx.scrollY) q.scrollY = ctx.scrollY
  return q
}

export function parseReturnContext(
  query: Record<string, unknown>
): DelegateFollowupReturnContext | null {
  const branchId = Number(query.branchId)
  const listId = Number(query.listId)
  if (!Number.isFinite(branchId) || branchId <= 0) return null
  if (!Number.isFinite(listId) || listId <= 0) return null

  const returnToRaw = String(query.returnTo ?? DELEGATE_FOLLOWUP_BOARD_ROUTE)
  const returnTo = ALLOWED_RETURN_ROUTES.has(returnToRaw)
    ? returnToRaw
    : DELEGATE_FOLLOWUP_BOARD_ROUTE

  return {
    returnTo,
    branchId,
    listId,
    filter: query.filter ? String(query.filter) : undefined,
    search: query.search ? String(query.search) : undefined,
    sort: query.sort ? String(query.sort) : undefined,
    page: query.page ? String(query.page) : undefined,
    scrollY: query.scrollY ? String(query.scrollY) : undefined,
  }
}
