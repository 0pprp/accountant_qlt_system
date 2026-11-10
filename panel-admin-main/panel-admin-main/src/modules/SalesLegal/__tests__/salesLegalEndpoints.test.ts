import { describe, expect, it } from 'vitest'
import { SalesLegalEndpoints } from '../domain/endpoints'

describe('SalesLegal FE contract', () => {
  it('exposes list create cancel followers', () => {
    expect(SalesLegalEndpoints.list).toBe('/admin/LegalFollowups')
    expect(SalesLegalEndpoints.create).toBe('/admin/LegalFollowups')
    expect(SalesLegalEndpoints.cancel(9)).toBe('/admin/LegalFollowups/9/cancel')
    expect(SalesLegalEndpoints.followers).toContain('followers')
  })
})
