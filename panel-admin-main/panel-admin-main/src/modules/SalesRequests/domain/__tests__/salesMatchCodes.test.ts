import { describe, expect, it } from 'vitest'
import {
  buildComparisonCodes,
  matchingTokens,
  normalizeName,
  phoneForMatch,
  tripleParts,
} from '../salesMatchCodes'

describe('salesMatchCodes', () => {
  it('normalizes Arabic name variants for comparison', () => {
    expect(normalizeName('أحمد  إبراهيم')).toBe('احمد ابراهيم')
    expect(matchingTokens('علي حسين محمد 12')).toEqual(['علي', 'حسين', 'محمد'])
  })

  it('builds phone match code from Iraqi formats', () => {
    expect(phoneForMatch('07801234567')).toBe('7801234567')
    expect(phoneForMatch('+9647801234567')).toBe('7801234567')
    expect(phoneForMatch('123')).toBe('')
  })

  it('exposes copyable comparison codes from request fields', () => {
    const codes = buildComparisonCodes('محمد علي حسين', '07801234567')
    const triple = codes.find((c) => c.criterion === 'tripleName')
    const phone = codes.find((c) => c.criterion === 'phone')
    expect(triple?.ready).toBe(true)
    expect(triple?.code).toBe('محمد · علي · حسين')
    expect(phone?.code).toBe('7801234567')
    expect(tripleParts('محمد علي حسين').father).toBe('علي')
  })
})
