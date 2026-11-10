import type { Router, RouteRecordRaw } from 'vue-router'
import { getReadyManagementTabs } from './domain/sections'
import { appRequirement } from './domain/workspaceNavigation'

function reuseLoader(reuse: string | null) {
  switch (reuse) {
    case 'CustomerView':
      return () => import('@/modules/Customer/views/CustomerView.vue')
    case 'PaymentListPage':
      return () => import('@/modules/Payment/views/PaymentListPage.vue')
    case 'SaleListPage':
      return () => import('@/modules/Sale/views/SaleListPage.vue')
    case 'SalesRequestsWorkspace':
      return () => import('./views/embeds/ManagementSalesRequestsEmbed.vue')
    case 'SalesFollowupPage':
      return () => import('@/modules/SalesFollowup/views/SalesFollowupPage.vue')
    case 'UserListPage':
      return () => import('@/modules/User/views/UserListPage.vue')
    case 'ActivityLogListPage':
      return () => import('@/modules/ActivityLog/views/ActivityLogListPage.vue')
    case 'ManagementTasksPage':
      return () => import('@/modules/ManagementTasks/views/ManagementTasksPage.vue')
    case 'DelegateFollowupPage':
      return () =>
        import('@/modules/DelegateFollowup/views/DelegateFollowupPage.vue')
    case 'ManagementSummaryPage':
      return () =>
        import('@/modules/ManagementSummary/views/ManagementSummaryPage.vue')
    case 'ManagementRatingsPage':
      return () =>
        import('@/modules/ManagementRatings/views/ManagementRatingsPage.vue')
    case 'FollowerWorkPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerWorkPage.vue')
    case 'FollowerSalesReviewPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerSalesReviewPage.vue')
    case 'FollowerDecliningPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerDecliningPage.vue')
    case 'FollowerNotesPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerNotesPage.vue')
    case 'FollowerOldCustomersPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerOldCustomersPage.vue')
    case 'FollowerDelegateNotesPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerDelegateNotesPage.vue')
    case 'FollowerCollectionPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerCollectionPage.vue')
    case 'FollowerLiveMapPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerLiveMapPage.vue')
    case 'FollowerRoutesPage':
      return () =>
        import('@/modules/FollowerWork/views/FollowerRoutesPage.vue')
    case 'CommitteeTasksPage':
      return () =>
        import('@/modules/CommitteeTasks/views/CommitteeTasksPage.vue')
    case 'AppNotesPage':
      return () => import('@/modules/AppNotes/views/AppNotesPage.vue')
    case 'WaiversPage':
      return () => import('@/modules/Waivers/views/WaiversPage.vue')
    case 'LegalNamesPage':
      return () => import('@/modules/CustomerDecisions/views/LegalNamesPage.vue')
    case 'CustomerDecisionsPage':
      return () =>
        import('@/modules/CustomerDecisions/views/CustomerDecisionsPage.vue')
    case 'PriceBoardPage':
      return () => import('@/modules/PriceBoard/views/PriceBoardPage.vue')
    case 'AnnouncementsPage':
      return () =>
        import('@/modules/Announcements/views/AnnouncementsPage.vue')
    case 'TutorialVideosPage':
      return () =>
        import('@/modules/TutorialVideos/views/TutorialVideosPage.vue')
    case 'WrongSalesPage':
      return () => import('@/modules/WrongSales/views/WrongSalesPage.vue')
    case 'BranchNewSalesPage':
      return () =>
        import('@/modules/BranchNewSales/views/BranchNewSalesPage.vue')
    case 'BranchDecliningPage':
      return () =>
        import('@/modules/BranchDeclining/views/BranchDecliningPage.vue')
    case 'ComplaintsPage':
      return () => import('@/modules/Complaints/views/ComplaintsPage.vue')
    case 'BranchVisitsPage':
      return () =>
        import('@/modules/BranchVisits/views/BranchVisitsPage.vue')
    case 'BranchManagerAccountsPage':
      return () =>
        import(
          '@/modules/BranchManagerAccounts/views/BranchManagerAccountsPage.vue'
        )
    case 'DelegateHomePage':
      return () =>
        import('@/modules/DelegateHome/views/DelegateHomePage.vue')
    case 'DelegateSettingsPage':
      return () =>
        import('@/modules/DelegateSettings/views/DelegateSettingsPage.vue')
    case 'BranchCustomersPage':
      return () =>
        import('@/modules/BranchCustomers/views/BranchCustomersPage.vue')
    case 'BranchPaymentsPage':
      return () =>
        import('@/modules/BranchPayments/views/BranchPaymentsPage.vue')
    default:
      return () => import('@/modules/Core/views/AccessDeniedView.vue')
  }
}

const readyChildren: RouteRecordRaw[] = getReadyManagementTabs().map((tab) => ({
  path: `${tab.sectionId}/${tab.tabPath}`,
  name: tab.routeName,
  component: reuseLoader(tab.reuse),
  props:
    tab.reuse === 'ManagementTasksPage' && ['branch', 'delegate'].includes(tab.sectionId)
      ? { inboxOnly: true }
      : tab.reuse === 'SalesRequestsWorkspace'
      ? () => ({
          defaultTab: tab.defaultQuery?.tab ?? undefined,
        })
      : tab.reuse === 'WrongSalesPage'
      ? { audience: tab.sectionId === 'branch' ? 'branch' : 'delegate' }
      : tab.reuse === 'BranchVisitsPage'
      ? { mode: tab.sectionId === 'branch' ? 'workspace' : 'history' }
      : undefined,
  meta: {
    requiresAuth: true,
    accessRequirement: tab.requirement,
    managementSectionId: tab.sectionId,
    managementTabId: tab.id,
  },
}))

/** صفحة تفاصيل الزبائن — ليست تبويب سايدبار؛ تُفتح من اللوحة مع return context */
const detailRoutes: RouteRecordRaw[] = [
  {
    path: 'director/delegate-followup/customers',
    name: 'ManagementDirectorDelegateFollowupCustomersRoute',
    component: () =>
      import(
        '@/modules/DelegateFollowup/views/DelegateFollowupCustomersPage.vue'
      ),
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'DelegateFollowup', action: 'Read' },
      },
      managementSectionId: 'director',
      managementTabId: 'delegateFollowup',
    },
  },
]

/** aliases آمنة — نفس الـguard والمكوّن بلا redirect loop */
const aliases: RouteRecordRaw[] = [
  {
    path: 'tasks',
    name: 'ManagementTasksCanonicalRoute',
    redirect: { name: 'ManagementDelegateAssignedTasksRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'Task', action: 'Read' },
      },
    },
  },
  {
    path: 'oversight/delegate-followup',
    name: 'ManagementOversightDelegateFollowupAliasRoute',
    redirect: { name: 'ManagementDirectorDelegateFollowupRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'DelegateFollowup', action: 'Read' },
      },
    },
  },
  {
    path: 'summary',
    name: 'ManagementSummaryCanonicalAliasRoute',
    redirect: { name: 'ManagementSummaryRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'ManagementSummary', action: 'Read' },
      },
    },
  },
  {
    path: 'ratings',
    name: 'ManagementRatingsCanonicalAliasRoute',
    redirect: { name: 'ManagementRatingsRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'ManagementRatings', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work',
    name: 'ManagementFollowerWorkCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerWorkRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
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
    },
  },
  {
    path: 'follower-work/sales',
    name: 'ManagementFollowerSalesCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerSalesRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerSalesReview', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/declining',
    name: 'ManagementFollowerDecliningCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerDecliningRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerDeclining', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/notes',
    name: 'ManagementFollowerNotesCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerNotesRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerNotes', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/old-customers',
    name: 'ManagementFollowerOldCustomersCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerOldCustomersRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerOldCustomers', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/delegate-notes',
    name: 'ManagementFollowerDelegateNotesCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerDelegateNotesRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerDelegateNotes', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/collection',
    name: 'ManagementFollowerCollectionCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerCollectionRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerCollection', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/live-map',
    name: 'ManagementFollowerLiveMapCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerLiveMapRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerLocation', action: 'Read' },
      },
    },
  },
  {
    path: 'follower-work/routes',
    name: 'ManagementFollowerRoutesCanonicalAliasRoute',
    redirect: { name: 'ManagementFollowerRoutesRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'FollowerRoute', action: 'Read' },
      },
    },
  },
  {
    path: 'branch/sale-followup',
    name: 'ManagementBranchSaleFollowupAliasKeep',
    redirect: { name: 'ManagementBranchSaleFollowupRoute' },
    meta: {
      requiresAuth: true,
      accessRequirement: {
        type: 'single',
        permission: { module: 'SalesFollowup', action: 'Read' },
      },
    },
  },
]

export const ManagementRoutes: RouteRecordRaw = {
  path: '/management',
  name: 'ManagementMainRoute',
  component: () => import('./views/ManagementWorkspace.vue'),
  redirect: { name: 'ManagementEntryRoute' },
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'anyOf',
      permissions: [
        { module: 'Customer', action: 'Read' },
        { module: 'OrderList', action: 'Read' },
        { module: 'Order', action: 'Read' },
        { module: 'SalesRequest', action: 'ReadUnassigned' },
        { module: 'SalesRequest', action: 'TabIncoming' },
        { module: 'SalesRequest', action: 'TabPrepared' },
        { module: 'SalesRequest', action: 'TabInspected' },
        { module: 'SalesRequest', action: 'TabPending' },
        { module: 'SalesRequest', action: 'TabRejected' },
        { module: 'SalesRequest', action: 'TabSold' },
        { module: 'SalesRequest', action: 'TabSent' },
        { module: 'SalesRequest', action: 'TabExceptions' },
        { module: 'SalesRequest', action: 'TabDeleted' },
        { module: 'SalesRequest', action: 'ExceptionDecide' },
        { module: 'SalesFollowup', action: 'Read' },
        { module: 'DelegateFollowup', action: 'Read' },
        { module: 'ManagementSummary', action: 'Read' },
        { module: 'ManagementRatings', action: 'Read' },
        { module: 'FollowerWork', action: 'Read' },
        { module: 'FollowerSalesReview', action: 'Read' },
        { module: 'FollowerDeclining', action: 'Read' },
        { module: 'FollowerNotes', action: 'Read' },
        { module: 'FollowerOldCustomers', action: 'Read' },
        { module: 'User', action: 'Read' },
        { module: 'User', action: 'ManagePermissions' },
        { module: 'ActivityLog', action: 'Read' },
        { module: 'Task', action: 'Read' },
      ],
    },
  },
  children: [
    ...(['branch', 'delegate'] as const).map(section => ({
      path: section,
      name: section === 'branch' ? 'ManagementBranchEntryRoute' : 'ManagementDelegateEntryRoute',
      component: () => import('./views/ManagementEntryRedirect.vue'),
      meta: { requiresAuth: true, accessRequirement: appRequirement(section), managementSectionId: section },
    })),
    {
      path: '',
      name: 'ManagementEntryRoute',
      component: () => import('./views/ManagementEntryRedirect.vue'),
      meta: {
        requiresAuth: true,
        accessRequirement: { type: 'publicAuthenticated' },
      },
    },
    ...readyChildren,
    ...detailRoutes,
    ...aliases,
  ],
}

export default (router: Router) => {
  router.addRoute(ManagementRoutes)
}
