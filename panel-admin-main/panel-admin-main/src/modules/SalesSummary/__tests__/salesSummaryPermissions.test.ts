import { describe, expect, it } from 'vitest'
import { SalesSummaryEndpoints } from '../domain/endpoints'

describe('SalesSummary FE contract', () => {
  it('uses dedicated endpoints not ManagementSummary', () => {
    expect(SalesSummaryEndpoints.matrix).toBe('/admin/SalesSummary/matrix')
    expect(SalesSummaryEndpoints.details).toBe('/admin/SalesSummary/details')
    expect(SalesSummaryEndpoints.matrix).not.toContain('ManagementSummary')
  })
})
