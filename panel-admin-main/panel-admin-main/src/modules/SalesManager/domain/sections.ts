import type { AccessRequirement } from '@/modules/Core/access/access.types'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'
import { SALES_REQUEST_TAB_PERMISSIONS } from '@/modules/Core/navigation/salesRequestPermissions'

export type SalesManagerSection =
  | 'overview'
  | 'requests'
  | 'rating'
  | 'followup'
  | 'live-map'
  | 'routes'
  | 'sales'
  | 'summary'
  | 'legal'
  | 'excel-search'

export interface SalesManagerSectionDef {
  key: SalesManagerSection
  label: string
  routeName: string
  path: string
  permissions: { module: PermissionModule; action: PermissionAction }[]
  employeeScoped: boolean
}

export const SALES_MANAGER_SECTIONS: readonly SalesManagerSectionDef[] = [
  { key: 'overview', label: 'نظرة عامة', routeName: 'SaleOverviewRoute', path: 'overview', permissions: [{ module: 'SalesOverview', action: 'Read' }], employeeScoped: false },
  { key: 'requests', label: 'طلبات البيع', routeName: 'SalesRequestsWorkspaceRoute', path: 'requests', permissions: [], employeeScoped: false },
  { key: 'rating', label: 'موظفو المبيعات', routeName: 'SaleEmployeesRoute', path: 'employees', permissions: [{ module: 'SalesEmployee', action: 'Read' }], employeeScoped: true },
  { key: 'followup', label: 'متابعة المبيعات', routeName: 'SalesFollowupRoute', path: 'followup', permissions: [{ module: 'SalesFollowup', action: 'Read' }], employeeScoped: false },
  { key: 'live-map', label: 'الموقع المباشر', routeName: 'SaleLiveMapRoute', path: 'live-map', permissions: [{ module: 'SalesLocation', action: 'Read' }], employeeScoped: false },
  { key: 'routes', label: 'المسارات', routeName: 'SaleRoutesRoute', path: 'routes', permissions: [{ module: 'SalesRoute', action: 'Read' }], employeeScoped: true },
  { key: 'sales', label: 'المبيعات المنجزة', routeName: 'SaleCompletedRoute', path: 'completed', permissions: [{ module: 'Order', action: 'Read' }], employeeScoped: false },
  { key: 'summary', label: 'ملخص المبيعات', routeName: 'SaleSummaryRoute', path: 'summary', permissions: [{ module: 'SalesSummary', action: 'Read' }], employeeScoped: false },
  { key: 'legal', label: 'المتابعة القانونية للمبيعات', routeName: 'SaleLegalFollowupRoute', path: 'legal', permissions: [{ module: 'LegalFollowup', action: 'Read' }], employeeScoped: false },
  { key: 'excel-search', label: 'بحث Excel للزبائن', routeName: 'SaleExcelSearchRoute', path: 'excel-search', permissions: [{ module: 'Customer', action: 'Read' }], employeeScoped: false },
] as const

export const SALES_MANAGER_ACCESS: AccessRequirement = {
  type: 'anyOf',
  permissions: [
    { module: 'Order', action: 'Read' },
    ...SALES_REQUEST_TAB_PERMISSIONS,
    { module: 'SalesOverview', action: 'Read' },
    { module: 'SalesEmployee', action: 'Read' },
    { module: 'SalesFollowup', action: 'Read' },
    { module: 'SalesLocation', action: 'Read' },
    { module: 'SalesRoute', action: 'Read' },
    { module: 'SalesSummary', action: 'Read' },
    { module: 'LegalFollowup', action: 'Read' },
    { module: 'Customer', action: 'Read' },
  ],
}

export function sectionByRouteName(name: unknown): SalesManagerSectionDef | null {
  return SALES_MANAGER_SECTIONS.find((s) => s.routeName === name) ?? null
}
