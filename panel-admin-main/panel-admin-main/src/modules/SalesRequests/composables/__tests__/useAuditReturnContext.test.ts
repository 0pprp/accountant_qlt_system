import { describe, expect, it } from 'vitest'
import {
  buildAuditReturnQuery,
  isAllowedReturnRoute,
  parseAuditReturnQuery,
} from '../useAuditReturnContext'

describe('useAuditReturnContext', () => {
  it('allows only known internal return routes', () => {
    expect(isAllowedReturnRoute('UnassignedSalesRequestsRoute')).toBe(true)
    expect(isAllowedReturnRoute('SalesRequestsWorkspaceRoute')).toBe(true)
    expect(isAllowedReturnRoute('SalesFollowupRoute')).toBe(true)
    expect(isAllowedReturnRoute('SaleCompletedRoute')).toBe(true)
    expect(isAllowedReturnRoute('SaleRoutesRoute')).toBe(true)
    expect(isAllowedReturnRoute('https://evil.example')).toBe(false)
    expect(isAllowedReturnRoute('SaleListRoute')).toBe(false)
  })

  it('round-trips audit context via query', () => {
    const query = buildAuditReturnQuery({
      branchId: '1',
      page: '2',
      requestId: '9',
      criterion: 'phone',
      searchSource: 'priorAccounts',
      scrollY: '120',
    })
    const parsed = parseAuditReturnQuery(query as Record<string, unknown>)
    expect(parsed.requestId).toBe('9')
    expect(parsed.criterion).toBe('phone')
    expect(parsed.scrollY).toBe('120')
  })
})
