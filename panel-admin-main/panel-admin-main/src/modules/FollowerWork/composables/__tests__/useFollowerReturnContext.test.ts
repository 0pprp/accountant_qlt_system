import { describe, expect, it } from 'vitest'
import {
  buildFollowerReturnTo,
  sanitizeFollowerReturnTo,
} from '../useFollowerReturnContext'

describe('useFollowerReturnContext', () => {
  it('يرفض URL خارجيًا ومسارًا غير مسموح', () => {
    expect(sanitizeFollowerReturnTo('https://evil.example/x')).toBeNull()
    expect(sanitizeFollowerReturnTo('//evil.example')).toBeNull()
    expect(sanitizeFollowerReturnTo('/sale/orders')).toBeNull()
    expect(sanitizeFollowerReturnTo('\\windows\\path')).toBeNull()
  })

  it('يقبل مسارات عمل المتابعين فقط', () => {
    expect(sanitizeFollowerReturnTo('/management/follower-work')).toBe(
      '/management/follower-work'
    )
    expect(
      sanitizeFollowerReturnTo('/management/follower-work?branchId=1&followerId=2')
    ).toBe('/management/follower-work?branchId=1&followerId=2')
    expect(
      sanitizeFollowerReturnTo('/management/oversight/follower-work?page=2')
    ).toBe('/management/oversight/follower-work?page=2')
  })

  it('يبني returnTo من query مسموح', () => {
    expect(buildFollowerReturnTo({ branchId: '3', followerId: '9' })).toContain(
      'branchId=3'
    )
  })
})
