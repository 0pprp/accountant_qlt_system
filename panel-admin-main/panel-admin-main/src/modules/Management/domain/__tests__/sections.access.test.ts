import { describe, expect, it } from 'vitest'
import {
  getReadyTabsForSection,
  getTabByIds,
  MANAGEMENT_TABS,
  TASK_ANY_PERMISSIONS,
} from '../sections'
import { evaluateAccess } from '@/modules/Core/access/evaluateAccess'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'

function hasOnly(owned: Array<[PermissionModule, PermissionAction]>) {
  const set = new Set(owned.map(([m, a]) => `${m}:${a}`))
  return (module: PermissionModule, action: PermissionAction) =>
    set.has(`${module}:${action}`)
}

describe('Management sections — Read vs action', () => {
  it('تبويب المهام يتطلب Task.Read فقط وليس Create/Assign', () => {
    const tab = getTabByIds('delegate', 'assigned-tasks')!
    expect(tab.requirement).toEqual({
      type: 'single',
      permission: { module: 'Task', action: 'Read' },
    })

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['Task', 'Create']]),
        requirement: tab.requirement,
      })
    ).toBe('denied')

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['Task', 'Read']]),
        requirement: tab.requirement,
      })
    ).toBe('allowed')
  })

  it('TASK_ANY للتوافق لا يُستخدم كشرط دخول التبويب', () => {
    const tab = getTabByIds('branch', 'assigned-tasks')!
    expect(tab.requirement.type).toBe('single')
    expect(TASK_ANY_PERMISSIONS.length).toBeGreaterThan(1)
  })

  it('متابعة المندوبين: Upsert بدون Read لا يفتح التبويب', () => {
    const tab = getTabByIds('director', 'delegate-followup')!
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([
          ['DelegateFollowup', 'UpsertBoardRating'],
        ]),
        requirement: tab.requirement,
      })
    ).toBe('denied')
  })

  it('مصادقة البيع والمتدنية ready', () => {
    const sales = MANAGEMENT_TABS.find((t) => t.id === 'followerSales')!
    expect(sales.implementationStatus).toBe('ready')
    expect(
      getReadyTabsForSection('oversight').some((t) => t.id === 'followerSales')
    ).toBe(true)

    const declining = MANAGEMENT_TABS.find((t) => t.id === 'followerDeclining')!
    expect(declining.implementationStatus).toBe('ready')

    const shell = MANAGEMENT_TABS.find((t) => t.id === 'followerWork')!
    expect(shell.implementationStatus).toBe('ready')
  })

  it('دليل المتابعين يُفتح بـ FollowerWork.Read دون صلاحية قسم', () => {
    const tab = getTabByIds('oversight', 'follower-work')!
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['FollowerWork', 'Read']]),
        requirement: tab.requirement,
      })
    ).toBe('allowed')
  })

  it('alias oversight/director يجد متابعة المندوبين', () => {
    expect(getTabByIds('oversight', 'delegate-followup')?.id).toBe(
      'delegateFollowup'
    )
    expect(getReadyTabsForSection('oversight').some((t) => t.id === 'delegateFollowup')).toBe(
      true
    )
  })
})
