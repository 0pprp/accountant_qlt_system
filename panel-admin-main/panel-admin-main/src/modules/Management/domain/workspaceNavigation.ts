import type { AccessRequirement, PermissionRef } from '@/modules/Core/access/access.types'
import type { NavigationSectionDefinition } from '@/modules/Core/navigation/navigation.types'
import { getReadyTabsForSection, getTabByIds, type ManagementTabDefinition } from './sections'

export type AppWorkspaceId = 'branch' | 'delegate'
export interface AppTab { id: string; label: string; icon: string; tabId?: string }
// Source order: branch_manager_application/lib/ui/home_shell.dart and
// delegated_manager_application/lib/features/shell/presentation/home_shell.dart.
// A missing tabId is unavailable, never a permission lock or substitute workflow.
export const APP_TABS: Record<AppWorkspaceId, AppTab[]> = {
  branch: [
    { id: 'tasks', label: 'مهامي', icon: 'pi-check-square', tabId: 'branchAssignedTasks' },
    { id: 'legal', label: 'أسماء قانونية', icon: 'pi-shield', tabId: 'legalNames' },
    { id: 'declining', label: 'الزبائن المتدنية', icon: 'pi-chart-line', tabId: 'branchDeclining' },
    { id: 'customers', label: 'الزبائن', icon: 'pi-users', tabId: 'customers' },
    { id: 'sales', label: 'المبيعات', icon: 'pi-shopping-bag', tabId: 'sales' },
    { id: 'wrongSales', label: 'المبيعات الخاطئة', icon: 'pi-exclamation-circle', tabId: 'wrongSalesBranch' },
    { id: 'ratings', label: 'التقييم', icon: 'pi-star', tabId: 'branchRatings' },
    { id: 'payments', label: 'التسديدات', icon: 'pi-wallet', tabId: 'payments' },
    { id: 'waivers', label: 'طلبات التنازل', icon: 'pi-file-edit', tabId: 'branchWaivers' },
    { id: 'history', label: 'سجل القرارات', icon: 'pi-history', tabId: 'legalDecisions' },
  ],
  delegate: [
    { id: 'home', label: 'الرئيسية', icon: 'pi-home', tabId: 'delegateHome' },
    { id: 'tasks', label: 'مهامي', icon: 'pi-check-square', tabId: 'assignedTasks' },
    { id: 'ratings', label: 'التقييمات', icon: 'pi-star', tabId: 'delegateRatings' },
    { id: 'complaints', label: 'الرسائل والشكاوى', icon: 'pi-comments', tabId: 'complaints' },
    { id: 'exceptions', label: 'طلبات الاستثناء', icon: 'pi-inbox', tabId: 'exceptions' },
    { id: 'wrongSales', label: 'المبيعات الخاطئة', icon: 'pi-exclamation-circle', tabId: 'wrongSalesDelegate' },
    { id: 'waivers', label: 'طلبات التنازل', icon: 'pi-file-edit', tabId: 'delegateWaivers' },
    { id: 'accounts', label: 'حسابات إدارة الفرع', icon: 'pi-id-card', tabId: 'accounts' },
    { id: 'visits', label: 'أعمال إدارة الفرع', icon: 'pi-building', tabId: 'branchVisits' },
    { id: 'settings', label: 'الإعدادات', icon: 'pi-cog', tabId: 'delegateSettings' },
  ],
}
export function appReadyTabs(section: AppWorkspaceId): ManagementTabDefinition[] {
  return APP_TABS[section].flatMap(item => {
    const tab = item.tabId ? getTabByIds(section, item.tabId) : undefined
    return tab?.implementationStatus === 'ready' ||
      tab?.implementationStatus === 'partial'
      ? [tab]
      : []
  })
}
function anyRead(tabs: ManagementTabDefinition[]): AccessRequirement {
  const permissions = tabs.flatMap(t => t.requirement.type === 'single'
    ? [t.requirement.permission] : t.requirement.type === 'publicAuthenticated' ? [] : [...t.requirement.permissions])
  if (!permissions.length) throw new Error('Workspace requires explicit permissions')
  return { type: 'anyOf', permissions: permissions as [PermissionRef, ...PermissionRef[]] }
}
export const oversightRequirement = anyRead(getReadyTabsForSection('oversight'))
export function appRequirement(section: AppWorkspaceId) { return anyRead(appReadyTabs(section)) }
export const appNavigationParents: NavigationSectionDefinition[] = (['branch', 'delegate'] as const).map((section, index) => ({
  id: section === 'branch' ? 'managementBranch' : 'managementDelegate',
  translationKey: `management.sections.${section}`,
  icon: section === 'branch' ? 'branch' : 'user', group: 'management',
  routeName: section === 'branch' ? 'ManagementBranchEntryRoute' : 'ManagementDelegateEntryRoute',
  routePath: `/management/${section}`, requirement: appRequirement(section),
  displayOrder: 66 + index, implementationStatus: 'ready',
  children: appReadyTabs(section).map((tab, order) => ({
    id: `app-${section}-${tab.id}`, parentId: section === 'branch' ? 'managementBranch' : 'managementDelegate',
    translationKey: `management.appTabs.${section}.${APP_TABS[section].find(item => item.tabId === tab.id)!.id}`,
    icon: 'user', group: 'management', routeName: tab.routeName,
    routePath: `/management/${section}/${tab.tabPath}`, routeQuery: tab.defaultQuery,
    requirement: tab.requirement, displayOrder: order,
    implementationStatus: tab.implementationStatus === 'partial' ? 'partial' : 'ready',
  })),
}))
