import { beforeEach, describe, expect, it, vi } from 'vitest'
import {
  EXTRA_VIEWS,
  EXTRA_VIEW_PERMISSION_ACTIONS,
  TAB_PERMISSION_ACTIONS,
  WORKFLOW_PERMISSION_ACTIONS,
  WORKFLOW_TABS,
} from '../../domain/tabs'

const granted = vi.hoisted(() => ({ actions: new Set<string>() }))

vi.mock('@/modules/Core/composable/usePermission', () => ({
  usePermission: () => ({
    can: (module: string, action: string) =>
      module === 'SalesRequest' && granted.actions.has(action),
  }),
}))

import { useSalesRequestTabAccess } from '../useSalesRequestTabAccess'

function grant(...actions: string[]) {
  granted.actions = new Set(actions)
}

describe('useSalesRequestTabAccess', () => {
  beforeEach(() => grant('ReadUnassigned', 'TabIncoming'))

  it('يفتح الحاوية إن وُجد تبويب واحد مسموح', () => {
    const { canOpenWorkspace, allowedWorkflowTabs, canReadTab } =
      useSalesRequestTabAccess()
    expect(canOpenWorkspace.value).toBe(true)
    expect(allowedWorkflowTabs.value.map((t) => t.key)).toEqual([
      'unassigned',
      'incoming',
    ])
    expect(canReadTab('prepared')).toBe(false)
    expect(canReadTab('all')).toBe(true)
  })

  it('legal جاهز ويُقفل بدون TabLegal', () => {
    grant('ReadUnassigned')
    const { isLocked, canReadTab } = useSalesRequestTabAccess()
    const legal = EXTRA_VIEWS.find((t) => t.key === 'legal')!
    expect(legal.implementationStatus).toBe('ready')
    expect(isLocked(legal)).toBe(true)
    expect(canReadTab('legal')).toBe(false)
  })

  it('TabLegal يفتح تبويب القانونية', () => {
    grant('TabLegal')
    const { canReadTab, isLocked } = useSalesRequestTabAccess()
    const legal = EXTRA_VIEWS.find((t) => t.key === 'legal')!
    expect(canReadTab('legal')).toBe(true)
    expect(isLocked(legal)).toBe(false)
  })

  it('sent/exceptions/deleted جاهزة ومقفلة بدون صلاحيتها', () => {
    const { isLocked } = useSalesRequestTabAccess()
    for (const key of ['sent', 'exceptions', 'deleted']) {
      const def = EXTRA_VIEWS.find((t) => t.key === key)!
      expect(def.implementationStatus).toBe('ready')
      expect(isLocked(def)).toBe(true)
    }
  })

  it('صلاحية تبويب إضافي تفتح هذا التبويب فقط', () => {
    grant('TabExceptions')
    const { canReadTab, canOpenWorkspace, resolveInitialTab } =
      useSalesRequestTabAccess()
    expect(canOpenWorkspace.value).toBe(true)
    expect(canReadTab('exceptions')).toBe(true)
    expect(canReadTab('sent')).toBe(false)
    expect(canReadTab('deleted')).toBe(false)
    expect(resolveInitialTab(null)).toBe('exceptions')
  })

  it('«الكل» لا يُفتح بصلاحيات العروض الإضافية وحدها', () => {
    grant('TabSent', 'TabDeleted')
    const { canReadTab, visibleExtraViews, canReadAnyWorkflowTab } =
      useSalesRequestTabAccess()
    expect(canReadAnyWorkflowTab.value).toBe(false)
    expect(canReadTab('all')).toBe(false)
    expect(visibleExtraViews.value.map((t) => t.key)).not.toContain('all')
    expect(canReadTab('sent')).toBe(true)
    expect(canReadTab('deleted')).toBe(true)
  })

  it('صلاحيات التبويب مستقلة في السجل', () => {
    expect(TAB_PERMISSION_ACTIONS).toContain('TabSold')
    expect(TAB_PERMISSION_ACTIONS).toEqual([
      ...WORKFLOW_PERMISSION_ACTIONS,
      ...EXTRA_VIEW_PERMISSION_ACTIONS,
    ])
    expect(EXTRA_VIEW_PERMISSION_ACTIONS).toEqual([
      'TabSent',
      'TabExceptions',
      'TabDeleted',
      'TabLegal',
    ])
    expect(TAB_PERMISSION_ACTIONS).not.toContain('ExceptionDecide')
    expect(WORKFLOW_TABS.every((t) => t.group === 'workflow')).toBe(true)
  })

  it('أكشنات القرار منفصلة عن تبويب الاستثناءات', () => {
    grant('TabExceptions', 'RequestException')
    const { canDecideException, canException } = useSalesRequestTabAccess()
    expect(canException.value).toBe(true)
    expect(canDecideException.value).toBe(false)
  })
})
