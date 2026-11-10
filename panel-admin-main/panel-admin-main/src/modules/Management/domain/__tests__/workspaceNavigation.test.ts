import { describe, expect, it } from 'vitest'
import { createRouter, createMemoryHistory } from 'vue-router'
import { APP_TABS, appNavigationParents, appRequirement } from '../workspaceNavigation'
import { NAVIGATION_REGISTRY } from '@/modules/Core/navigation/navigation.registry'
import { ManagementRoutes } from '../../router'
import { evaluateAccess } from '@/modules/Core/access/evaluateAccess'
import { resolveRouteAccess } from '@/modules/Core/access/resolveRouteAccess'

describe('Independent legacy application workspaces', () => {
  it('branch and delegated manager are roots, never children of oversight', () => {
    for (const id of ['managementBranch', 'managementDelegate']) {
      expect(NAVIGATION_REGISTRY.filter(x => x.id === id)).toHaveLength(1)
      expect(NAVIGATION_REGISTRY.find(x => x.id === 'management')!.children!.some(x => x.id === id)).toBe(false)
    }
  })
  it('preserves the ten source tabs in each app with wired destinations', () => {
    expect(APP_TABS.branch).toHaveLength(10)
    expect(APP_TABS.delegate).toHaveLength(10)
    expect(APP_TABS.branch.find(x => x.id === 'declining')!.tabId).toBe('branchDeclining')
    expect(APP_TABS.branch.find(x => x.id === 'ratings')!.tabId).toBe('branchRatings')
    expect(APP_TABS.branch.find(x => x.id === 'wrongSales')!.tabId).toBe('wrongSalesBranch')
    expect(APP_TABS.branch.find(x => x.id === 'history')!.tabId).toBe('legalDecisions')
    expect(APP_TABS.delegate.find(x => x.id === 'home')!.tabId).toBe('delegateHome')
    expect(APP_TABS.delegate.find(x => x.id === 'ratings')!.tabId).toBe('delegateRatings')
    expect(APP_TABS.delegate.find(x => x.id === 'complaints')!.tabId).toBe('complaints')
    expect(APP_TABS.delegate.find(x => x.id === 'wrongSales')!.tabId).toBe('wrongSalesDelegate')
    expect(APP_TABS.delegate.find(x => x.id === 'accounts')!.tabId).toBe('accounts')
    expect(APP_TABS.delegate.find(x => x.id === 'visits')!.tabId).toBe('branchVisits')
    expect(APP_TABS.delegate.find(x => x.id === 'settings')!.tabId).toBe('delegateSettings')
  })
  it('wired workflows appear as actionable sidebar links', () => {
    const delegateRoutes = appNavigationParents[1].children!.map(x => x.routeName)
    expect(delegateRoutes).toContain('ManagementDelegateBranchVisitsRoute')
    expect(delegateRoutes).toContain('ManagementDelegateComplaintsRoute')
    expect(delegateRoutes).toContain('ManagementDelegateAccountsRoute')
    expect(delegateRoutes).toContain('ManagementDelegateHomeRoute')
  })
  it('general user management does not unlock the delegated-manager app', () => {
    expect(evaluateAccess({ permissionsReady: true, isAuthenticated: true,
      hasPermission: (module) => module === 'User', requirement: appRequirement('delegate') })).toBe('denied')
  })
  it('old and new routes retain leaf access checks; actions alone cannot read tasks', () => {
    const router = createRouter({ history: createMemoryHistory(), routes: [ManagementRoutes] })
    for (const path of ['/management/delegate/assigned-tasks', '/management/branch/assigned-tasks']) {
      const to = router.resolve(path)
      expect(to.matched.length).toBeGreaterThan(1)
      expect(resolveRouteAccess({ to, isAuthenticated: true, permissionsStatus: 'ready',
        hasPermission: (module, action) => module === 'Task' && action === 'Create' }).type).toBe('denied')
      expect(resolveRouteAccess({ to, isAuthenticated: true, permissionsStatus: 'ready',
        hasPermission: (module, action) => module === 'Task' && action === 'Read' }).type).toBe('allow')
    }
  })
  it('a branch-only reader cannot open the delegated manager root', () => {
    const router = createRouter({ history: createMemoryHistory(), routes: [ManagementRoutes] })
    const access = (path: string) => resolveRouteAccess({ to: router.resolve(path), isAuthenticated:true,
      permissionsStatus:'ready', hasPermission: (module, action) => module === 'Customer' && action === 'Read' }).type
    expect(access('/management/branch')).toBe('allow')
    expect(access('/management/delegate')).toBe('denied')
  })
  it('branch manager accounts require BranchManagerAccount not ManagePermissions', () => {
    const router = createRouter({ history: createMemoryHistory(), routes: [ManagementRoutes] })
    const path = '/management/delegate/accounts'
    expect(resolveRouteAccess({
      to: router.resolve(path),
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: (module, action) =>
        module === 'User' && (action === 'Read' || action === 'ManagePermissions'),
    }).type).toBe('denied')
    expect(resolveRouteAccess({
      to: router.resolve(path),
      isAuthenticated: true,
      permissionsStatus: 'ready',
      hasPermission: (module, action) =>
        module === 'BranchManagerAccount' && action === 'Read',
    }).type).toBe('allow')
  })
})
