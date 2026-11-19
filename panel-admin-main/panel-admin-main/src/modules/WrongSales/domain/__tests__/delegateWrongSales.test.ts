import { describe, expect, it } from 'vitest'
import { normalizeDelegateMailbox, wrongSaleDate, wrongSaleDateTime } from '../delegateWrongSales'

describe('normalizeDelegateMailbox', () => {
  it('defaults to unread and keeps known chips', () => {
    expect(normalizeDelegateMailbox(undefined)).toBe('unread')
    expect(normalizeDelegateMailbox('unread')).toBe('unread')
    expect(normalizeDelegateMailbox('seen')).toBe('seen')
    expect(normalizeDelegateMailbox('read')).toBe('seen')
    expect(normalizeDelegateMailbox('reviewed')).toBe('reviewed')
    expect(normalizeDelegateMailbox('all')).toBe('unread')
  })
})

describe('wrong sale dates', () => {
  it('formats date and time', () => {
    expect(wrongSaleDateTime('2026-11-18T06:05:00Z')).toBe('2026/11/18 09:05 ص')
    expect(wrongSaleDateTime('2026-11-18T21:00:00Z')).toBe('2026/11/19 12:00 ص')
    expect(wrongSaleDate('2026-10-05')).toBe('2026/10/05')
    expect(wrongSaleDate(null)).toBe('—')
  })
})
