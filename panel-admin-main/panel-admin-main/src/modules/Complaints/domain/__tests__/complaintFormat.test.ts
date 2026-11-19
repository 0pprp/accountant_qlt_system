import { describe, expect, it } from 'vitest'
import { complaintRelativeTime, complaintRoleLabel, complaintSourceLabel } from '../complaintFormat'

describe('complaintSourceLabel', () => {
  it('maps known source apps', () => {
    expect(complaintSourceLabel('delegate-app')).toBe('مندوب')
    expect(complaintSourceLabel('follower')).toBe('متابع')
    expect(complaintSourceLabel('sales-employee')).toBe('موظف مبيعات')
    expect(complaintSourceLabel('filter')).toBe('موظف فلترة')
    expect(complaintSourceLabel('Complaint')).toBe('شكوى')
    expect(complaintSourceLabel('Message')).toBe('رسالة')
    expect(complaintSourceLabel('')).toBe('غير متوفر')
    expect(complaintSourceLabel(null)).toBe('غير متوفر')
  })

  it('uses sender role for committee sources', () => {
    expect(complaintSourceLabel('committee', 'Motaba')).toBe('متابع')
    expect(complaintSourceLabel('committee', 'Mandob')).toBe('مندوب')
    expect(complaintSourceLabel('committee', '')).toBe('تطبيق اللجنة')
  })
})

describe('complaintRoleLabel', () => {
  it('translates role codes and falls back to raw text', () => {
    expect(complaintRoleLabel('Mandob')).toBe('مندوب')
    expect(complaintRoleLabel('BranchManager')).toBe('مدير الفرع')
    expect(complaintRoleLabel('Custom')).toBe('Custom')
    expect(complaintRoleLabel('')).toBe('غير متوفر')
  })
})

describe('complaintRelativeTime', () => {
  const now = new Date('2026-11-18T12:00:00Z')

  it('formats recent times relatively', () => {
    expect(complaintRelativeTime('2026-11-18T11:59:30Z', now)).toBe('الآن')
    expect(complaintRelativeTime('2026-11-18T11:45:00Z', now)).toBe('قبل 15 دقيقة')
    expect(complaintRelativeTime('2026-11-18T09:00:00Z', now)).toBe('قبل 3 ساعة')
    expect(complaintRelativeTime('2026-11-10T12:00:00Z', now)).toBe('قبل 8 يوم')
  })

  it('shows a date after 30 days and handles bad input', () => {
    const old = complaintRelativeTime('2026-09-01T12:00:00Z', now)
    expect(old).not.toMatch(/^قبل/)
    expect(old).toMatch(/2026/)
    expect(complaintRelativeTime(null, now)).toBe('—')
    expect(complaintRelativeTime('nope', now)).toBe('—')
  })
})
