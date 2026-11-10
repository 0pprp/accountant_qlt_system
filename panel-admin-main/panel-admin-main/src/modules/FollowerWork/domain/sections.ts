import type { FollowerSectionDef } from './types'

/**
 * سجل أقسام عمل المتابعين — منسجم مع السجل المركزي (navigation.registry + Management/sections).
 * ليس مصدر صلاحيات ثانيًا؛ الحالة هنا للعرض داخل الهيكل فقط.
 * أقسام 06–13 ready.
 */
export const FOLLOWER_WORK_SECTIONS: readonly FollowerSectionDef[] = [
  {
    id: 'sales',
    pathSegment: 'sales',
    labelKey: 'sections.sales',
    permissionModule: 'FollowerSalesReview',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'declining',
    pathSegment: 'declining',
    labelKey: 'sections.declining',
    permissionModule: 'FollowerDeclining',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'notes',
    pathSegment: 'notes',
    labelKey: 'sections.notes',
    permissionModule: 'FollowerNotes',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'old-customers',
    pathSegment: 'old-customers',
    labelKey: 'sections.oldCustomers',
    permissionModule: 'FollowerOldCustomers',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'delegate-notes',
    pathSegment: 'delegate-notes',
    labelKey: 'sections.delegateNotes',
    permissionModule: 'FollowerDelegateNotes',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'collection',
    pathSegment: 'collection',
    labelKey: 'sections.collection',
    permissionModule: 'FollowerCollection',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'live-map',
    pathSegment: 'live-map',
    labelKey: 'sections.liveMap',
    permissionModule: 'FollowerLocation',
    permissionAction: 'Read',
    status: 'ready',
  },
  {
    id: 'routes',
    pathSegment: 'routes',
    labelKey: 'sections.routes',
    permissionModule: 'FollowerRoute',
    permissionAction: 'Read',
    status: 'ready',
  },
] as const

export function getImplementedSections(): FollowerSectionDef[] {
  return FOLLOWER_WORK_SECTIONS.filter((s) => s.status === 'ready')
}

export const FOLLOWER_WORK_CANONICAL_PATH = '/management/follower-work'
export const FOLLOWER_WORK_ROUTE_NAME = 'ManagementFollowerWorkRoute'
