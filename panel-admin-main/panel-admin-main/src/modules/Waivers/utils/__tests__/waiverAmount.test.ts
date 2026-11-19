import { describe, expect, it } from 'vitest'
import { formatWaiverAmountInput, parseWaiverAmount, waiverAmountDigits } from '../waiverAmount'

describe('waiverAmount', () => {
  it('formats with thousands separators while typing', () => {
    expect(formatWaiverAmountInput('1500000')).toBe('1,500,000')
    expect(formatWaiverAmountInput('1,500,00')).toBe('150,000')
    expect(formatWaiverAmountInput('999')).toBe('999')
    expect(formatWaiverAmountInput('')).toBe('')
  })

  it('accepts Arabic-Indic digits and drops other characters', () => {
    expect(formatWaiverAmountInput('١٢٣٤٥')).toBe('12,345')
    expect(waiverAmountDigits('12a3 د.ع')).toBe('123')
    expect(waiverAmountDigits('000250')).toBe('250')
  })

  it('parses only positive whole amounts', () => {
    expect(parseWaiverAmount('1,500,000')).toBe(1500000)
    expect(parseWaiverAmount('0')).toBeNull()
    expect(parseWaiverAmount('000')).toBeNull()
    expect(parseWaiverAmount('')).toBeNull()
    expect(parseWaiverAmount('abc')).toBeNull()
    expect(parseWaiverAmount('-500')).toBe(500)
    expect(parseWaiverAmount('99999999999999999999')).toBeNull()
  })
})
