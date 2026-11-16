import { describe, expect, it } from 'vitest'
import {
  MANAGEMENT_TAB_TO_PANEL_TAB,
  SIDEBAR_ID_TO_PANEL_TAB,
  SIDEBAR_ID_TO_PANEL_TABS_ANY,
  SALES_REQUEST_PANEL_TAB_KEYS,
  panelTabRequirement,
  requirementForNavSectionId,
} from '../panelTabAccess'
import { getAllNavigationSections } from '../navigation.registry'

describe('panelTabAccess — مفاتيح التبويب', () => {
  it('لا تكرار في مفاتيح السايدبار الأحادية', () => {
    const values = Object.values(SIDEBAR_ID_TO_PANEL_TAB)
    expect(new Set(values).size).toBe(values.length)
  })

  it('لا تكرار في مفاتيح إدارة اللوحة', () => {
    const values = Object.values(MANAGEMENT_TAB_TO_PANEL_TAB)
    expect(new Set(values).size).toBe(values.length)
  })

  it('كل مفتاح طلب بيع فريد', () => {
    expect(new Set(SALES_REQUEST_PANEL_TAB_KEYS).size).toBe(
      SALES_REQUEST_PANEL_TAB_KEYS.length
    )
  })

  it('حاوية المبيعات تستخدم anyOf لمفاتيح PanelTab', () => {
    const req = requirementForNavSectionId('sale', {
      type: 'publicAuthenticated',
    })
    expect(req.type).toBe('anyOf')
    if (req.type !== 'anyOf') return
    expect(req.permissions.every((p) => p.module === 'PanelTab')).toBe(true)
  })

  it('فرع السايدبار يتحول إلى PanelTab.branch', () => {
    expect(panelTabRequirement('branch')).toEqual({
      type: 'single',
      permission: { module: 'PanelTab', action: 'branch' },
    })
    expect(
      requirementForNavSectionId('branch', {
        type: 'single',
        permission: { module: 'Branch', action: 'Read' },
      })
    ).toEqual(panelTabRequirement('branch'))
  })

  it('كل id أحادي في السجل له مفتاح أو حاوية anyOf أو مسار app-*', () => {
    const missing: string[] = []
    for (const section of getAllNavigationSections()) {
      if (section.children?.length) continue
      if (SIDEBAR_ID_TO_PANEL_TAB[section.id]) continue
      if (SIDEBAR_ID_TO_PANEL_TABS_ANY[section.id]) continue
      if (
        section.id.startsWith('app-branch-') ||
        section.id.startsWith('app-delegate-')
      ) {
        continue
      }
      // حاويات بلا أبناء ظاهرة نادرة — تُقبل إن بقي requirement صريح
      if (section.id === 'saleRequests' || section.id === 'managementFollowerWork')
        continue
      missing.push(section.id)
    }
    expect(missing).toEqual([])
  })
})
