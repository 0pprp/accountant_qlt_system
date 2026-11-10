import type { AccessRequirement } from '@/modules/Core/access/access.types'

export type NavigationGroupId =
  | 'overview'
  | 'operations'
  | 'finance'
  | 'inventory'
  | 'people'
  | 'system'
  | 'management'
  | 'sales'

export type NavigationImplementationStatus =
  | 'ready'
  | 'partial'
  | 'not_implemented'

export interface NavigationSectionDefinition {
  /** مفتاح ثابت (stableId) — لا يُترجم ولا يعتمد على اسم الدور */
  id: string
  /** أب اختياري في الشجرة المنطقية (للتوثيق والتجميع) */
  parentId?: string | null
  translationKey: string
  icon: string
  group: NavigationGroupId
  routeName: string
  /** مسار URL اختياري للتوثيق/aliases */
  routePath?: string
  /** استعلام اختياري عند التنقل (مثل tab=sales) — يُرفض إن لم يكن الابن مخوّلًا */
  routeQuery?: Record<string, string>
  requirement: AccessRequirement
  displayOrder: number
  implementationStatus: NavigationImplementationStatus
  children?: readonly NavigationSectionDefinition[]
  /** بادج اختياري (مثل الإشعارات) يُملأ في useSideBar */
  supportsBadge?: boolean
  /** ملاحظة قبول / حدود النطاق */
  notes?: string
}

export type NavigationItemState = 'allowed' | 'locked' | 'hidden' | 'unavailable'

export interface NavigationSidebarItem {
  children?: NavigationSidebarItem[]
  id: string
  title: string
  icon: string
  value: string
  link: string
  query?: Record<string, string>
  group: NavigationGroupId
  state: NavigationItemState
  badgeCount?: number
  displayOrder: number
}
