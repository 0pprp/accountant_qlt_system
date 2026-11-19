import { describe, expect, it } from 'vitest'
import { visitBranchLabel, visitDateTime, visitMembersLine, visitStatus } from '../visitHistory'

describe('visitStatus', () => {
  it('maps every status to an Arabic label', () => {
    expect(visitStatus('Open')).toEqual({ label: 'مفتوحة', open: true })
    expect(visitStatus('Departed')).toEqual({ label: 'مغلقة', open: false })
    expect(visitStatus('Weird').label).toBe('غير معروفة')
    expect(visitStatus(null).label).toBe('غير معروفة')
  })
})

describe('visitDateTime', () => {
  it('formats in Baghdad time with Arabic AM/PM', () => {
    expect(visitDateTime('2026-11-18T06:05:00Z')).toBe('2026/11/18 09:05 ص')
    expect(visitDateTime('2026-11-18T12:30:00Z')).toBe('2026/11/18 03:30 م')
    expect(visitDateTime(null)).toBe('—')
  })
})

describe('visit helpers', () => {
  it('prefers branch name and joins members', () => {
    expect(visitBranchLabel({ branchId: 3, branchName: 'فرع الكرخ' })).toBe('فرع الكرخ')
    expect(visitBranchLabel({ branchId: 3, branchName: null }, 'فرع بديل')).toBe('فرع بديل')
    expect(visitBranchLabel({ branchId: 3, branchName: null })).toBe('فرع غير معروف')
    expect(visitMembersLine([{ displayName: 'أ' }, { displayName: ' ' }, { displayName: 'ب' }])).toBe('أ، ب')
    expect(visitMembersLine([])).toBe('—')
  })
})
