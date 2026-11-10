import type { AccessRequirement } from '@/modules/Core/access/access.types'
import type { NavigationSectionDefinition } from './navigation.types'

import { SALES_REQUEST_TAB_PERMISSIONS } from './salesRequestPermissions'
export { SALES_REQUEST_TAB_PERMISSIONS } from './salesRequestPermissions'
import { appNavigationParents, oversightRequirement } from '@/modules/Management/domain/workspaceNavigation'

const salesRequestAnyOf: AccessRequirement = {
  type: 'anyOf',
  permissions: SALES_REQUEST_TAB_PERMISSIONS,
}

/**
 * تعريف مركزي لأقسام اللوحة.
 * شروط الوصول من صلاحيات الموارد (وليس أسماء الأدوار).
 * not_implemented يُوثَّق هنا ولا يظهر كقفل صلاحية في السايدبار.
 */
export const NAVIGATION_REGISTRY: readonly NavigationSectionDefinition[] = [
  {
    id: 'home',
    translationKey: 'sidebar.home',
    icon: 'home',
    group: 'overview',
    routeName: 'HomeMainRoute',
    routePath: '/',
    requirement: { type: 'publicAuthenticated' },
    displayOrder: 10,
    implementationStatus: 'ready',
  },
  {
    id: 'branch',
    translationKey: 'sidebar.branch',
    icon: 'branch',
    group: 'operations',
    routeName: 'BranchListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Branch', action: 'Read' },
    },
    displayOrder: 20,
    implementationStatus: 'ready',
  },
  {
    id: 'payment',
    translationKey: 'sidebar.payment',
    icon: 'payment',
    group: 'operations',
    routeName: 'PaymentListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'OrderList', action: 'Read' },
    },
    displayOrder: 30,
    implementationStatus: 'ready',
  },
  {
    id: 'customer',
    translationKey: 'sidebar.customer',
    icon: 'customer',
    group: 'operations',
    routeName: 'CustomerMainRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Customer', action: 'Read' },
    },
    displayOrder: 40,
    implementationStatus: 'ready',
  },
  {
    id: 'purchases',
    translationKey: 'sidebar.purchases',
    icon: 'purchases',
    group: 'finance',
    routeName: 'PurchasesListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Purchase', action: 'Read' },
    },
    displayOrder: 50,
    implementationStatus: 'ready',
  },

  // —— المبيعات (أب واحد) ——
  {
    id: 'sale',
    translationKey: 'sidebar.sale',
    icon: 'sale',
    group: 'sales',
    routeName: 'SaleMainRoute',
    routePath: '/sale',
    requirement: {
      type: 'anyOf',
      permissions: [
        { module: 'Order', action: 'Read' },
        ...SALES_REQUEST_TAB_PERMISSIONS,
        { module: 'SalesFollowup', action: 'Read' },
        { module: 'SalesOverview', action: 'Read' },
        { module: 'SalesEmployee', action: 'Read' },
        { module: 'SalesLocation', action: 'Read' },
      ],
    },
    displayOrder: 60,
    implementationStatus: 'ready',
    notes: 'أب وظيفي واحد للمبيعات — بلا لوحة حسب الدور',
    children: [
      {
        id: 'saleOverview',
        parentId: 'sale',
        translationKey: 'sidebar.saleOverview',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleOverviewRoute',
        routePath: '/sale/overview',
        requirement: {
          type: 'single',
          permission: { module: 'SalesOverview', action: 'Read' },
        },
        displayOrder: 60,
        implementationStatus: 'ready',
        notes: 'Source: sales-manager-dashboard — برومبت 14',
      },
      {
        id: 'saleRequests',
        parentId: 'sale',
        translationKey: 'sidebar.saleRequests',
        icon: 'sale',
        group: 'sales',
        routeName: 'SalesRequestsWorkspaceRoute',
        routePath: '/sale/requests',
        requirement: salesRequestAnyOf,
        displayOrder: 61,
        implementationStatus: 'ready',
      },
      {
        id: 'saleEmployees',
        parentId: 'sale',
        translationKey: 'sidebar.saleEmployees',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleEmployeesRoute',
        routePath: '/sale/employees',
        requirement: {
          type: 'single',
          permission: { module: 'SalesEmployee', action: 'Read' },
        },
        displayOrder: 62,
        implementationStatus: 'ready',
        notes: 'Source: sales-manager-employees — برومبت 15',
      },
      {
        id: 'saleFollowup',
        parentId: 'sale',
        translationKey: 'sidebar.saleFollowup',
        icon: 'sale',
        group: 'sales',
        routeName: 'SalesFollowupRoute',
        routePath: '/sale/followup',
        requirement: {
          type: 'single',
          permission: { module: 'SalesFollowup', action: 'Read' },
        },
        displayOrder: 63,
        implementationStatus: 'ready',
        notes: 'موظفو المبيعات (SellerId) — ليست متابعة المندوبين',
      },
      {
        id: 'saleLiveMap',
        parentId: 'sale',
        translationKey: 'sidebar.saleLiveMap',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleLiveMapRoute',
        routePath: '/sale/live-map',
        requirement: {
          type: 'single',
          permission: { module: 'SalesLocation', action: 'Read' },
        },
        displayOrder: 64,
        implementationStatus: 'ready',
        notes: 'Source: sales-manager-map — برومبت 17 — ActorKind=SalesEmployee',
      },
      {
        id: 'saleRoutes',
        parentId: 'sale',
        translationKey: 'sidebar.saleRoutes',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleRoutesRoute',
        routePath: '/sale/routes',
        requirement: {
          type: 'single',
          permission: { module: 'SalesRoute', action: 'Read' },
        },
        displayOrder: 65,
        implementationStatus: 'ready',
        notes: 'Source: sales-manager-routes — برومبت 18 — يوم تقويمي بغداد',
      },
      {
        id: 'saleOrders',
        parentId: 'sale',
        translationKey: 'sidebar.saleOrders',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleCompletedRoute',
        routePath: '/sale/completed',
        requirement: {
          type: 'single',
          permission: { module: 'Order', action: 'Read' },
        },
        displayOrder: 66,
        implementationStatus: 'ready',
        notes: 'المبيعات المنجزة — Orders معتمدة /SaleDate — برومبت 19',
      },
      {
        id: 'saleSummary',
        parentId: 'sale',
        translationKey: 'sidebar.saleSummary',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleSummaryRoute',
        routePath: '/sale/summary',
        requirement: {
          type: 'single',
          permission: { module: 'SalesSummary', action: 'Read' },
        },
        displayOrder: 67,
        implementationStatus: 'ready',
        notes: 'سعر البيع فقط — برومبت 20 — معزول عن ManagementSummary',
      },
      {
        id: 'saleLegal',
        parentId: 'sale',
        translationKey: 'sidebar.saleLegal',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleLegalFollowupRoute',
        routePath: '/sale/legal',
        requirement: {
          type: 'single',
          permission: { module: 'LegalFollowup', action: 'Read' },
        },
        displayOrder: 68,
        implementationStatus: 'ready',
        notes: 'LegalFollowupCases دورة عمل — برومبت 21',
      },
      {
        id: 'saleExcelSearch',
        parentId: 'sale',
        translationKey: 'sidebar.saleExcelSearch',
        icon: 'sale',
        group: 'sales',
        routeName: 'SaleExcelSearchRoute',
        routePath: '/sale/excel-search',
        requirement: {
          type: 'single',
          permission: { module: 'Customer', action: 'Read' },
        },
        displayOrder: 69,
        implementationStatus: 'ready',
        notes: 'Source: sales-manager-excel-search — برومبت 09',
      },
    ],
  },

  {
    id: 'warehouse',
    translationKey: 'sidebar.warehouse',
    icon: 'warehouse',
    group: 'inventory',
    routeName: 'WarehouseListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'ProductCategory', action: 'Read' },
    },
    displayOrder: 70,
    implementationStatus: 'ready',
  },
  {
    id: 'safeMain',
    translationKey: 'sidebar.safeMain',
    icon: 'safe',
    group: 'finance',
    routeName: 'SafeMainMainRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Safe', action: 'Read' },
    },
    displayOrder: 80,
    implementationStatus: 'ready',
  },
  {
    id: 'safe',
    translationKey: 'sidebar.safe',
    icon: 'safe',
    group: 'finance',
    routeName: 'SafeMainRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Safe', action: 'Read' },
    },
    displayOrder: 90,
    implementationStatus: 'ready',
  },
  {
    id: 'order',
    translationKey: 'sidebar.order',
    icon: 'order',
    group: 'operations',
    routeName: 'OrderListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'OrderList', action: 'Read' },
    },
    displayOrder: 100,
    implementationStatus: 'ready',
  },

  // —— لوحة الإدارة (أب واحد للمدير/المراقب وظيفيًا) ——
  {
    id: 'management',
    translationKey: 'sidebar.management',
    icon: 'user',
    group: 'management',
    routeName: 'ManagementMainRoute',
    routePath: '/management',
    requirement: oversightRequirement,
    displayOrder: 65,
    implementationStatus: 'ready',
    notes: 'أب واحد — بلا لوحة مدير/مراقب منفصلة؛ التسميات وظيفية',
    children: [
      {
        id: 'managementSummary',
        parentId: 'management',
        translationKey: 'sidebar.managementSummary',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementSummaryRoute',
        routePath: '/management/summary',
        requirement: {
          type: 'single',
          permission: { module: 'ManagementSummary', action: 'Read' },
        },
        displayOrder: 105,
        implementationStatus: 'ready',
        notes:
          'Source: director-summary — ManagementSummary.Read؛ ليس ملخص المبيعات',
      },
      {
        id: 'managementRatings',
        parentId: 'management',
        translationKey: 'sidebar.managementRatings',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementRatingsRoute',
        routePath: '/management/ratings',
        requirement: {
          type: 'single',
          permission: { module: 'ManagementRatings', action: 'Read' },
        },
        displayOrder: 106,
        implementationStatus: 'ready',
        notes:
          'Source: director-ratings — ManagementRatings.Read؛ ليست SalesFollowup',
      },
      {
        id: 'managementDelegateFollowup',
        parentId: 'management',
        translationKey: 'sidebar.managementDelegateFollowup',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementDirectorDelegateFollowupRoute',
        routePath: '/management/director/delegate-followup',
        requirement: {
          type: 'single',
          permission: { module: 'DelegateFollowup', action: 'Read' },
        },
        displayOrder: 107,
        implementationStatus: 'ready',
        notes: 'دخول التبويب = Read فقط؛ UpsertBoardRating إجراء داخل الصفحة',
      },
      {
        id: 'managementFollowerWork',
        parentId: 'management',
        translationKey: 'sidebar.managementFollowerWork',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementFollowerWorkRoute',
        routePath: '/management/follower-work',
        requirement: {
          type: 'anyOf',
          permissions: [
            { module: 'FollowerWork', action: 'Read' },
            { module: 'FollowerSalesReview', action: 'Read' },
            { module: 'FollowerDeclining', action: 'Read' },
            { module: 'FollowerNotes', action: 'Read' },
            { module: 'FollowerOldCustomers', action: 'Read' },
            { module: 'FollowerDelegateNotes', action: 'Read' },
            { module: 'FollowerCollection', action: 'Read' },
            { module: 'FollowerLocation', action: 'Read' },
            { module: 'FollowerRoute', action: 'Read' },
          ],
        },
        displayOrder: 108,
        implementationStatus: 'ready',
        notes:
          'هيكل + بطاقات؛ أقسام 06–13 ready',
        children: [
          {
            id: 'followerSales',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerSales',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerSalesRoute',
            routePath: '/management/follower-work/sales',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerSalesReview', action: 'Read' },
            },
            displayOrder: 1,
            implementationStatus: 'ready',
            notes: 'قراءة نتائج المصادقة؛ الكتابة من تطبيق اللجنة فقط',
          },
          {
            id: 'followerDeclining',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerDeclining',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerDecliningRoute',
            routePath: '/management/follower-work/declining',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerDeclining', action: 'Read' },
            },
            displayOrder: 2,
            implementationStatus: 'ready',
            notes: 'هبوط درجتين خلال 7 أيام من لقطات التقييم — قراءة فقط',
          },
          {
            id: 'followerNotes',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerNotes',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerNotesRoute',
            routePath: '/management/follower-work/notes',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerNotes', action: 'Read' },
            },
            displayOrder: 3,
            implementationStatus: 'ready',
            notes: 'CustomerNotes — قراءة لوحة؛ كتابة تطبيق المتابع',
          },
          {
            id: 'followerOldCustomers',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerOldCustomers',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerOldCustomersRoute',
            routePath: '/management/follower-work/old-customers',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerOldCustomers', action: 'Read' },
            },
            displayOrder: 4,
            implementationStatus: 'ready',
            notes: 'LegalFollowupCases؛ Cancel منفصل',
          },
          {
            id: 'followerDelegateNotes',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerDelegateNotes',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerDelegateNotesRoute',
            routePath: '/management/follower-work/delegate-notes',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerDelegateNotes', action: 'Read' },
            },
            displayOrder: 5,
            implementationStatus: 'ready',
            notes: 'FollowerDelegateNotes — قراءة لوحة؛ كتابة تطبيق المتابع',
          },
          {
            id: 'followerCollection',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerCollection',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerCollectionRoute',
            routePath: '/management/follower-work/collection',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerCollection', action: 'Read' },
            },
            displayOrder: 6,
            implementationStatus: 'ready',
            notes: 'قراءة مستقلة؛ ManageEnabled منفصل',
          },
          {
            id: 'followerLiveMap',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerLiveMap',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerLiveMapRoute',
            routePath: '/management/follower-work/live-map',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerLocation', action: 'Read' },
            },
            displayOrder: 7,
            implementationStatus: 'ready',
            notes: 'آخر موقع حقيقي للمتابعين؛ poll 15s',
          },
          {
            id: 'followerRoutes',
            parentId: 'managementFollowerWork',
            translationKey: 'sidebar.followerRoutes',
            icon: 'user',
            group: 'management',
            routeName: 'ManagementFollowerRoutesRoute',
            routePath: '/management/follower-work/routes',
            requirement: {
              type: 'single',
              permission: { module: 'FollowerRoute', action: 'Read' },
            },
            displayOrder: 8,
            implementationStatus: 'ready',
            notes: 'مسار يوم بغداد — markers فقط',
          },
        ],
      },
      {
        id: 'managementCommitteeTasks',
        parentId: 'management',
        translationKey: 'sidebar.managementCommitteeTasks',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementCommitteeTasksRoute',
        routePath: '/management/oversight/committee-tasks',
        requirement: {
          type: 'single',
          permission: { module: 'CommitteeTask', action: 'Read' },
        },
        displayOrder: 110,
        implementationStatus: 'ready',
        notes: 'نشرة مهام اللجنة — CommitteeTask.Read؛ الكتابة Write',
      },
      {
        id: 'managementAppNotes',
        parentId: 'management',
        translationKey: 'sidebar.managementAppNotes',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementAppNotesRoute',
        routePath: '/management/oversight/app-notes',
        requirement: {
          type: 'single',
          permission: { module: 'AppNote', action: 'Read' },
        },
        displayOrder: 111,
        implementationStatus: 'ready',
        notes: 'وارد ملاحظات التطبيقات — AppNote.Read؛ Create من التطبيقات',
      },
      {
        id: 'managementWaivers',
        parentId: 'management',
        translationKey: 'sidebar.managementWaivers',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementWaiversRoute',
        routePath: '/management/oversight/waivers',
        requirement: {
          type: 'single',
          permission: { module: 'Waiver', action: 'Read' },
        },
        displayOrder: 112,
        implementationStatus: 'ready',
        notes: 'قراءة إشراف؛ Decide/Apply منفصلتان',
      },
      {
        id: 'managementLegalNames',
        parentId: 'management',
        translationKey: 'sidebar.managementLegalNames',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementLegalNamesRoute',
        routePath: '/management/branch/legal-names',
        requirement: {
          type: 'single',
          permission: { module: 'LegalNames', action: 'Read' },
        },
        displayOrder: 113,
        implementationStatus: 'ready',
        notes: 'تحت إدارة الفروع — طابور ضعاف الدفع؛ ليست LegalFollowup',
      },
      {
        id: 'managementPriceBoard',
        parentId: 'management',
        translationKey: 'sidebar.managementPriceBoard',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementPriceBoardRoute',
        routePath: '/management/oversight/price-board',
        requirement: {
          type: 'single',
          permission: { module: 'PriceBoard', action: 'Read' },
        },
        displayOrder: 114,
        implementationStatus: 'ready',
        notes: 'إعلانات أسعار محافظة — لا تعدّل Product/Order',
      },
      {
        id: 'managementAnnouncements',
        parentId: 'management',
        translationKey: 'sidebar.managementAnnouncements',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementAnnouncementsRoute',
        routePath: '/management/oversight/announcements',
        requirement: {
          type: 'single',
          permission: { module: 'Announcement', action: 'Read' },
        },
        displayOrder: 115,
        implementationStatus: 'ready',
        notes: 'بانر تطبيقات — ليست AppNote/Push',
      },
      {
        id: 'managementTutorials',
        parentId: 'management',
        translationKey: 'sidebar.managementTutorials',
        icon: 'user',
        group: 'management',
        routeName: 'ManagementTutorialsRoute',
        routePath: '/management/oversight/tutorials',
        requirement: {
          type: 'single',
          permission: { module: 'Tutorial', action: 'Read' },
        },
        displayOrder: 116,
        implementationStatus: 'ready',
        notes: 'أسئلة + روابط خارجية — جمهور التطبيقات الميدانية',
      },
    ],
  },

  ...appNavigationParents,

  {
    id: 'user',
    translationKey: 'sidebar.user',
    icon: 'user',
    group: 'people',
    routeName: 'UserListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'User', action: 'Read' },
    },
    displayOrder: 110,
    implementationStatus: 'ready',
  },
  {
    id: 'activityLog',
    translationKey: 'sidebar.activityLog',
    icon: 'activityLog',
    group: 'system',
    routeName: 'ActivityLogListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'ActivityLog', action: 'Read' },
    },
    displayOrder: 120,
    implementationStatus: 'ready',
  },
  {
    id: 'notifications',
    translationKey: 'sidebar.notifications',
    icon: 'notification',
    group: 'system',
    routeName: 'NotificationListRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Notification', action: 'Read' },
    },
    displayOrder: 130,
    implementationStatus: 'ready',
    supportsBadge: true,
  },
] as const

function walk(
  sections: readonly NavigationSectionDefinition[],
  visit: (section: NavigationSectionDefinition) => void
) {
  for (const section of sections) {
    visit(section)
    if (section.children?.length) walk(section.children, visit)
  }
}

export function getAllNavigationSections(): NavigationSectionDefinition[] {
  const all: NavigationSectionDefinition[] = []
  walk(NAVIGATION_REGISTRY, (s) => all.push(s))
  return all
}

export function getNavigationSectionById(
  id: string
): NavigationSectionDefinition | undefined {
  return getAllNavigationSections().find((section) => section.id === id)
}

export function getNavigationSectionByRouteName(
  routeName: string
): NavigationSectionDefinition | undefined {
  return getAllNavigationSections().find(
    (section) => section.routeName === routeName
  )
}

/** أقسام جاهزة للسايدبار — يستبعد not_implemented من الشجرة المعروضة */
export function getReadyNavigationSections(): NavigationSectionDefinition[] {
  return NAVIGATION_REGISTRY.map(filterReadyBranch).filter(
    (s): s is NavigationSectionDefinition => s != null
  )
}

function filterReadyBranch(
  section: NavigationSectionDefinition
): NavigationSectionDefinition | null {
  if (section.children?.length) {
    const readyChildren = section.children
      .map(filterReadyBranch)
      .filter((c): c is NavigationSectionDefinition => c != null)

    if (section.implementationStatus !== 'ready' && readyChildren.length === 0) {
      return null
    }

    // أب ready بلا أبناء جاهزين لكن له مسار خاص — أبقِه إن كان ready
    if (section.implementationStatus === 'ready' || readyChildren.length > 0) {
      return {
        ...section,
        implementationStatus: 'ready',
        children: readyChildren.length > 0 ? readyChildren : undefined,
      }
    }
    return null
  }

  return section.implementationStatus === 'ready' ? section : null
}

export function getDocumentedNotImplementedSections(): NavigationSectionDefinition[] {
  return getAllNavigationSections().filter(
    (s) => s.implementationStatus === 'not_implemented'
  )
}
