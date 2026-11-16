import type {
  AccessRequirement,
  PermissionRef,
} from '@/modules/Core/access/access.types'
import type { PermissionAction, PermissionModule } from '@/modules/Auth/types/model'

export const PANEL_TAB_MODULE = 'PanelTab' as const satisfies PermissionModule

export function panelTabRef(tabKey: string): PermissionRef {
  return {
    module: PANEL_TAB_MODULE,
    action: tabKey as PermissionAction,
  }
}

export function panelTabRequirement(tabKey: string): AccessRequirement {
  return { type: 'single', permission: panelTabRef(tabKey) }
}

export function panelTabsAnyOf(
  tabKeys: readonly [string, ...string[]]
): AccessRequirement {
  const permissions = tabKeys.map(panelTabRef) as [
    PermissionRef,
    ...PermissionRef[],
  ]
  return { type: 'anyOf', permissions }
}

/** تبويبات طلبات البيع — مفاتيح Panel.Tab */
export const SALES_REQUEST_PANEL_TAB_KEYS = [
  'sr.unassigned',
  'sr.incoming',
  'sr.prepared',
  'sr.inspected',
  'sr.pending',
  'sr.rejected',
  'sr.sold',
  'sr.sent',
  'sr.exceptions',
  'sr.deleted',
  'sr.legal',
] as const

export const SALE_CHILD_PANEL_TAB_KEYS = [
  'saleOverview',
  'saleEmployees',
  'saleFollowup',
  'saleLiveMap',
  'saleRoutes',
  'saleOrders',
  'saleSummary',
  'saleLegal',
  'saleExcelSearch',
  ...SALES_REQUEST_PANEL_TAB_KEYS,
] as const

export const FOLLOWER_PANEL_TAB_KEYS = [
  'followerSales',
  'followerDeclining',
  'followerNotes',
  'followerOldCustomers',
  'followerDelegateNotes',
  'followerCollection',
  'followerLiveMap',
  'followerRoutes',
] as const

export const MANAGEMENT_LEAF_PANEL_TAB_KEYS = [
  'managementSummary',
  'managementRatings',
  'managementDelegateFollowup',
  'managementSalesMap',
  'managementCommitteeTasks',
  'managementAppNotes',
  'managementWaivers',
  'managementLegalNames',
  'managementPriceBoard',
  'managementAnnouncements',
  'managementTutorials',
  ...FOLLOWER_PANEL_TAB_KEYS,
] as const

/** تطابق id السايدبار → مفتاح تبويب واحد */
export const SIDEBAR_ID_TO_PANEL_TAB: Readonly<Record<string, string>> = {
  home: 'home',
  branch: 'branch',
  payment: 'payment',
  customer: 'customer',
  purchases: 'purchases',
  warehouse: 'warehouse',
  safeMain: 'safeMain',
  safe: 'safe',
  order: 'order',
  user: 'user',
  activityLog: 'activityLog',
  notifications: 'notifications',
  saleOverview: 'saleOverview',
  saleEmployees: 'saleEmployees',
  saleFollowup: 'saleFollowup',
  saleLiveMap: 'saleLiveMap',
  saleRoutes: 'saleRoutes',
  saleOrders: 'saleOrders',
  saleSummary: 'saleSummary',
  saleLegal: 'saleLegal',
  saleExcelSearch: 'saleExcelSearch',
  managementSummary: 'managementSummary',
  managementRatings: 'managementRatings',
  managementSalesMap: 'managementSalesMap',
  managementDelegateFollowup: 'managementDelegateFollowup',
  managementCommitteeTasks: 'managementCommitteeTasks',
  managementAppNotes: 'managementAppNotes',
  managementWaivers: 'managementWaivers',
  managementLegalNames: 'managementLegalNames',
  managementPriceBoard: 'managementPriceBoard',
  managementAnnouncements: 'managementAnnouncements',
  managementTutorials: 'managementTutorials',
  followerSales: 'followerSales',
  followerDeclining: 'followerDeclining',
  followerNotes: 'followerNotes',
  followerOldCustomers: 'followerOldCustomers',
  followerDelegateNotes: 'followerDelegateNotes',
  followerCollection: 'followerCollection',
  followerLiveMap: 'followerLiveMap',
  followerRoutes: 'followerRoutes',
}

/** حاويات: أي ابن مفتوح يكفي لعرض الحاوية */
export const SIDEBAR_ID_TO_PANEL_TABS_ANY: Readonly<
  Record<string, readonly [string, ...string[]]>
> = {
  sale: SALE_CHILD_PANEL_TAB_KEYS as unknown as [string, ...string[]],
  saleRequests: SALES_REQUEST_PANEL_TAB_KEYS as unknown as [string, ...string[]],
  management: MANAGEMENT_LEAF_PANEL_TAB_KEYS as unknown as [
    string,
    ...string[],
  ],
  managementFollowerWork: FOLLOWER_PANEL_TAB_KEYS as unknown as [
    string,
    ...string[],
  ],
}

/**
 * تبويبات لوحة الإدارة الداخلية → Panel.Tab
 * (يشمل مفاتيح app-branch-* و app-delegate-*)
 */
export const MANAGEMENT_TAB_TO_PANEL_TAB: Readonly<Record<string, string>> = {
  summary: 'managementSummary',
  ratings: 'managementRatings',
  salesMap: 'managementSalesMap',
  delegateFollowup: 'managementDelegateFollowup',
  committeeTasks: 'managementCommitteeTasks',
  appNotes: 'managementAppNotes',
  waivers: 'managementWaivers',
  legalNames: 'managementLegalNames',
  priceBoard: 'managementPriceBoard',
  announcements: 'managementAnnouncements',
  tutorials: 'managementTutorials',
  followerSales: 'followerSales',
  followerDeclining: 'followerDeclining',
  followerNotes: 'followerNotes',
  followerOldCustomers: 'followerOldCustomers',
  followerDelegateNotes: 'followerDelegateNotes',
  followerCollection: 'followerCollection',
  followerLiveMap: 'followerLiveMap',
  followerRoutes: 'followerRoutes',
  branchAssignedTasks: 'app-branch-branchAssignedTasks',
  customers: 'app-branch-customers',
  payments: 'app-branch-payments',
  sales: 'app-branch-sales',
  wrongSalesBranch: 'app-branch-wrongSalesBranch',
  branchDeclining: 'app-branch-branchDeclining',
  branchRatings: 'app-branch-branchRatings',
  branchWaivers: 'app-branch-branchWaivers',
  legalDecisions: 'app-branch-legalDecisions',
  branchVisitWorkspace: 'branchVisitWorkspace',
  delegateHome: 'app-delegate-delegateHome',
  assignedTasks: 'app-delegate-assignedTasks',
  delegateRatings: 'app-delegate-delegateRatings',
  complaints: 'app-delegate-complaints',
  exceptions: 'app-delegate-exceptions',
  wrongSalesDelegate: 'app-delegate-wrongSalesDelegate',
  delegateWaivers: 'app-delegate-delegateWaivers',
  accounts: 'app-delegate-accounts',
  branchVisits: 'app-delegate-branchVisits',
  delegateSettings: 'app-delegate-delegateSettings',
  // مسارات مشتركة داخل الإدارة
  saleRequests: 'sr.incoming',
  saleFollowup: 'saleFollowup',
  users: 'user',
  activityLog: 'activityLog',
  salesEmployeeRating: 'saleEmployees',
}

export function requirementForNavSectionId(
  sectionId: string,
  fallback: AccessRequirement
): AccessRequirement {
  const anyKeys = SIDEBAR_ID_TO_PANEL_TABS_ANY[sectionId]
  if (anyKeys) return panelTabsAnyOf(anyKeys)

  const single = SIDEBAR_ID_TO_PANEL_TAB[sectionId]
  if (single) return panelTabRequirement(single)

  if (
    sectionId.startsWith('app-branch-') ||
    sectionId.startsWith('app-delegate-')
  ) {
    return panelTabRequirement(sectionId)
  }

  return fallback
}

export function requirementForManagementTabId(
  tabId: string,
  fallback: AccessRequirement
): AccessRequirement {
  const key = MANAGEMENT_TAB_TO_PANEL_TAB[tabId]
  if (key) return panelTabRequirement(key)
  return fallback
}

/** وصف أثر فتح التبويب — يظهر عند الحاجة فقط */
export const TAB_EFFECT_NOTES_AR: Readonly<Record<string, string>> = {
  managementWaivers:
    'يشمل اعتماد التنازل المالي وتطبيق أثره عند توفره ضمن عمليات هذا التبويب.',
  'app-delegate-delegateWaivers':
    'يشمل البت في طلبات التنازل للمفوض، بما فيها الاعتماد المالي عند الحاجة.',
  'app-branch-branchWaivers':
    'عرض ومتابعة تنازلات الفرع دون بالضرورة منح اعتماد مالي مستقل.',
  'sr.exceptions':
    'البت في الاستثناء من هذا التبويب فقط؛ تقديم الاستثناء يبقى من تبويبات الطلب غير المسند/الوارد.',
  'sr.unassigned':
    'يشمل الإسناد وتقديم استثناء من غير المسند عند السماح بحالة السجل.',
  'sr.incoming':
    'يشمل الإسناد والرفض وتقديم استثناء وفق حالة الطلب وقواعد العمل.',
  'app-delegate-exceptions':
    'البت في استثناءات المفوض — منفصل عن تقديم الاستثناء من شاشات الطلب.',
  safe: 'يشمل إنشاء وتعديل الحركات المالية ضمن الصندوق.',
  safeMain: 'عرض الصناديق الرئيسية وحركاتها دون بالضرورة منح تعديل الحركات.',
  user: 'إدارة الحسابات دون منح صلاحية تعديل تبويبات الآخرين تلقائيًا.',
  saleLiveMap: 'عرض مواقع المبيعات الحية على الخريطة.',
  managementSalesMap: 'خريطة مبيعات لوحة الإدارة.',
}

export const TAB_PARENT_LABELS_AR: Readonly<Record<string, string>> = {
  _root: 'الأقسام الرئيسية',
  sale: 'المبيعات',
  saleRequests: 'طلبات البيع',
  management: 'لوحة الإدارة',
  managementFollowerWork: 'عمل المتابعين',
  managementBranch: 'تطبيق الفرع',
  managementDelegate: 'تطبيق المفوض',
}

/** ترتيب الأقسام كما في السايدبار */
export const TAB_PARENT_ORDER: readonly string[] = [
  '_root',
  'sale',
  'saleRequests',
  'management',
  'managementFollowerWork',
  'managementBranch',
  'managementDelegate',
]
