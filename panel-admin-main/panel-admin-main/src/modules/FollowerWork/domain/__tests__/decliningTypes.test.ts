import { describe, expect, it } from 'vitest'
import {
  declineDirectionLabel,
  scoreDelta,
  type FollowerDecliningRow,
} from '../decliningTypes'

function row(partial: Partial<FollowerDecliningRow>): FollowerDecliningRow {
  return {
    customerId: 1,
    customerName: 'x',
    phone: null,
    listId: 1,
    listName: null,
    fromLabel: 'ممتاز',
    toLabel: 'ضعيف',
    ratingLabel: 'ضعيف',
    fromScore: 10,
    toScore: 0,
    daysBetween: 3,
    priorCaptureDate: '2026-11-01',
    currentCaptureDate: '2026-11-04',
    hasNote: null,
    lastNoteAtUtc: null,
    hasSaleAuth: false,
    ...partial,
  }
}

describe('decliningTypes direction', () => {
  it('درجة سالبة = تراجع لا تحسّن', () => {
    const r = row({ fromScore: 10, toScore: 0 })
    expect(scoreDelta(r)).toBe(-10)
    expect(declineDirectionLabel(r)).toBe('تراجع')
  })

  it('درجة موجبة = تحسّن', () => {
    const r = row({ fromScore: 0, toScore: 5 })
    expect(declineDirectionLabel(r)).toBe('تحسّن')
  })
})
