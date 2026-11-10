import { describe, expect, it } from 'vitest'
import {
  getDocumentedNotImplementedSections,
  getNavigationSectionById,
  getNavigationSectionByRouteName,
  getReadyNavigationSections,
  NAVIGATION_REGISTRY,
} from '../navigation.registry'
import { evaluateAccess } from '@/modules/Core/access/evaluateAccess'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'

function hasOnly(
  owned: Array<[PermissionModule, PermissionAction]>
) {
  const set = new Set(owned.map(([m, a]) => `${m}:${a}`))
  return (module: PermissionModule, action: PermissionAction) =>
    set.has(`${module}:${action}`)
}

describe('navigation.registry — توحيد الإدارة والمبيعات', () => {
  it('يحتوي أبًا واحدًا للإدارة وآخر للمبيعات', () => {
    const management = getNavigationSectionById('management')
    const sale = getNavigationSectionById('sale')
    expect(management?.translationKey).toBe('sidebar.management')
    expect(sale?.translationKey).toBe('sidebar.sale')
    expect(management?.children?.length).toBeGreaterThan(5)
    expect(sale?.children?.length).toBeGreaterThan(5)
  })

  it('لا يُظهر not_implemented في شجرة السايدبار الجاهزة', () => {
    const ready = getReadyNavigationSections()
    const flat: string[] = []
    const walk = (nodes: typeof ready) => {
      for (const n of nodes) {
        flat.push(n.id)
        if (n.children) walk([...n.children])
      }
    }
    walk(ready)
    expect(flat).toContain('managementSummary')
    expect(flat).toContain('managementRatings')
    expect(flat).toContain('managementFollowerWork')
    expect(flat).toContain('followerSales')
    expect(flat).toContain('followerDeclining')
    expect(flat).toContain('followerNotes')
    expect(flat).toContain('followerOldCustomers')
    expect(flat).toContain('followerDelegateNotes')
    expect(flat).toContain('followerCollection')
    expect(flat).toContain('followerLiveMap')
    expect(flat).toContain('followerRoutes')
    expect(flat).toContain('saleOverview')
    expect(flat).toContain('saleEmployees')
    expect(flat).toContain('saleLiveMap')
    expect(flat).toContain('saleRoutes')
    expect(flat).toContain('saleSummary')
    expect(flat).toContain('saleLegal')
    expect(flat).toContain('managementDelegateFollowup')
    expect(flat).toContain('saleFollowup')
    expect(flat).toContain('saleOrders')
  })

  it('يوثّق غير المنفَّذ دون جعله ready', () => {
    const documented = getDocumentedNotImplementedSections()
    expect(documented.some((s) => s.id === 'managementSummary')).toBe(false)
    expect(documented.some((s) => s.id === 'managementRatings')).toBe(false)
    expect(documented.some((s) => s.id === 'managementFollowerWork')).toBe(false)
    expect(documented.some((s) => s.id === 'followerSales')).toBe(false)
    expect(documented.some((s) => s.id === 'followerDeclining')).toBe(false)
    expect(documented.some((s) => s.id === 'followerNotes')).toBe(false)
    expect(documented.some((s) => s.id === 'followerOldCustomers')).toBe(false)
    expect(documented.some((s) => s.id === 'followerDelegateNotes')).toBe(false)
    expect(documented.some((s) => s.id === 'followerCollection')).toBe(false)
    expect(documented.some((s) => s.id === 'followerLiveMap')).toBe(false)
    expect(documented.some((s) => s.id === 'followerRoutes')).toBe(false)
    expect(documented.every((s) => s.implementationStatus === 'not_implemented')).toBe(
      true
    )
  })

  it('عمل المتابعين: الدليل بـ FollowerWork.Read؛ قسم غير منفّذ ليس قفل صلاحية جاهز', () => {
    const shell = getNavigationSectionById('managementFollowerWork')!
    expect(shell.implementationStatus).toBe('ready')
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['FollowerWork', 'Read']]),
        requirement: shell.requirement,
      })
    ).toBe('allowed')

    const salesChild = shell.children?.find((c) => c.id === 'followerSales')
    expect(salesChild?.implementationStatus).toBe('ready')
    const decliningChild = shell.children?.find((c) => c.id === 'followerDeclining')
    expect(decliningChild?.implementationStatus).toBe('ready')
    expect(
      shell.children?.find((c) => c.id === 'followerNotes')?.implementationStatus
    ).toBe('ready')
    expect(
      shell.children?.find((c) => c.id === 'followerOldCustomers')
        ?.implementationStatus
    ).toBe('ready')
    expect(
      shell.children?.find((c) => c.id === 'followerDelegateNotes')
        ?.implementationStatus
    ).toBe('ready')
    expect(
      shell.children?.find((c) => c.id === 'followerCollection')
        ?.implementationStatus
    ).toBe('ready')
    expect(
      shell.children?.find((c) => c.id === 'followerLiveMap')
        ?.implementationStatus
    ).toBe('ready')
    expect(
      shell.children?.find((c) => c.id === 'followerRoutes')
        ?.implementationStatus
    ).toBe('ready')
  })

  it('دخول التقييمات يتطلب ManagementRatings.Read — DetailsRead وحده لا يكفي', () => {
    const tab = getNavigationSectionById('managementRatings')!
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['ManagementRatings', 'DetailsRead']]),
        requirement: tab.requirement,
      })
    ).toBe('denied')

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['ManagementRatings', 'Read']]),
        requirement: tab.requirement,
      })
    ).toBe('allowed')
  })

  it('دخول الملخص الإداري يتطلب ManagementSummary.Read فقط', () => {
    const tab = getNavigationSectionById('managementSummary')!
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['SalesFollowup', 'Read']]),
        requirement: tab.requirement,
      })
    ).toBe('denied')

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['ManagementSummary', 'Read']]),
        requirement: tab.requirement,
      })
    ).toBe('allowed')

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['ManagementSummary', 'Drilldown']]),
        requirement: tab.requirement,
      })
    ).toBe('denied')
  })

  it('دخول متابعة المندوبين يتطلب Read فقط — Upsert لا يكفي', () => {
    const tab = getNavigationSectionById('managementDelegateFollowup')!
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

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: hasOnly([['DelegateFollowup', 'Read']]),
        requirement: tab.requirement,
      })
    ).toBe('allowed')
  })

  it('حساب يملك Read لابن واحد يفتح الأب ويبقي بقية الجاهز مقفولًا', () => {
    const management = getReadyNavigationSections().find((s) => s.id === 'management')!
    const has = hasOnly([['DelegateFollowup', 'Read']])

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: has,
        requirement: management.requirement,
      })
    ).toBe('allowed')

    const followup = management.children!.find(
      (c) => c.id === 'managementDelegateFollowup'
    )!
    const tasks = management.children!.find((c) => c.id === 'managementRatings')!
    const branch = getReadyNavigationSections().find((c) => c.id === 'managementBranch')!

    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: has,
        requirement: followup.requirement,
      })
    ).toBe('allowed')
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: has,
        requirement: tasks.requirement,
      })
    ).toBe('denied')
    expect(
      evaluateAccess({
        permissionsReady: true,
        isAuthenticated: true,
        hasPermission: has,
        requirement: branch.requirement,
      })
    ).toBe('denied')
  })

  it('يبحث عن المسار في الأبناء المتداخلين', () => {
    expect(
      getNavigationSectionByRouteName('ManagementDirectorDelegateFollowupRoute')
        ?.id
    ).toBe('managementDelegateFollowup')
    expect(getNavigationSectionByRouteName('SalesFollowupRoute')?.id).toBe(
      'saleFollowup'
    )
  })

  it('لا يكرر مصروفًا محذوفًا كمدخل مبيعات', () => {
    const sale = NAVIGATION_REGISTRY.find((s) => s.id === 'sale')!
    expect(sale.children?.some((c) => c.id.toLowerCase().includes('expense'))).toBe(
      false
    )
  })
})
