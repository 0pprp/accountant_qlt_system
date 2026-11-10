import type {
  AccessRequirement,
  PermissionRef,
} from '@/modules/Core/access/access.types'
import { SALES_REQUEST_TAB_PERMISSIONS } from '@/modules/Core/navigation/salesRequestPermissions'

/**
 * سجل مركزي لأقسام لوحة الإدارة وتبويباتها.
 * التبويبات الجاهزة فقط تُعرض كقفل صلاحية؛ غير المنفَّذ موثّق وليس قفل صلاحية.
 * تسميات الأقسام وظيفية — ليست أسماء أدوار (مدير/مراقب).
 */

export type ManagementSectionId =
  | 'oversight'
  | 'branch'
  | 'delegate'
  /** alias مستقر للمسارات القديمة /management/director/... */
  | 'director'

export type ManagementTabId =
  | 'customers'
  | 'payments'
  | 'sales'
  | 'saleRequests'
  | 'saleFollowup'
  | 'users'
  | 'activityLog'
  | 'branchAssignedTasks'
  | 'exceptions'
  | 'salesEmployeeRating'
  | 'accounts'
  | 'assignedTasks'
  | 'delegateFollowup'
  | 'summary'
  | 'ratings'
  | 'followerWork'
  | 'followerSales'
  | 'followerDeclining'
  | 'followerNotes'
  | 'followerOldCustomers'
  | 'followerDelegateNotes'
  | 'followerCollection'
  | 'followerLiveMap'
  | 'followerRoutes'
  | 'committeeTasks'
  | 'appNotes'
  | 'waivers'
  | 'branchWaivers'
  | 'delegateWaivers'
  | 'legalNames'
  | 'priceBoard'
  | 'announcements'
  | 'tutorials'
  | 'complaints'
  | 'branchVisits'
  | 'legalDecisions'
  | 'committeeRatings'
  | 'wrongSalesBranch'
  | 'wrongSalesDelegate'
  | 'branchDeclining'
  | 'branchRatings'
  | 'delegateHome'
  | 'delegateRatings'
  | 'delegateSettings'
  | 'branchVisitWorkspace'

export type ManagementImplementationStatus =
  | 'ready'
  | 'partial'
  | 'not_implemented'

export interface ManagementTabDefinition {
  id: ManagementTabId
  sectionId: ManagementSectionId
  /** مفتاح ترجمة ضمن management.* */
  labelKey: string
  /** مسار فرعي تحت /management/:sectionId/:tabId */
  tabPath: string
  routeName: string
  requirement: AccessRequirement
  implementationStatus: ManagementImplementationStatus
  reuse:
    | 'CustomerView'
    | 'PaymentListPage'
    | 'SaleListPage'
    | 'SalesRequestsWorkspace'
    | 'SalesFollowupPage'
    | 'UserListPage'
    | 'ActivityLogListPage'
    | 'ManagementTasksPage'
    | 'DelegateFollowupPage'
    | 'ManagementSummaryPage'
    | 'ManagementRatingsPage'
    | 'FollowerWorkPage'
    | 'FollowerSalesReviewPage'
    | 'FollowerDecliningPage'
    | 'FollowerNotesPage'
    | 'FollowerOldCustomersPage'
    | 'FollowerDelegateNotesPage'
    | 'FollowerCollectionPage'
    | 'FollowerLiveMapPage'
    | 'FollowerRoutesPage'
    | 'CommitteeTasksPage'
    | 'AppNotesPage'
    | 'WaiversPage'
    | 'LegalNamesPage'
    | 'CustomerDecisionsPage'
    | 'PriceBoardPage'
    | 'AnnouncementsPage'
    | 'TutorialVideosPage'
    | 'WrongSalesPage'
    | 'BranchNewSalesPage'
    | 'BranchDecliningPage'
    | 'ComplaintsPage'
    | 'BranchVisitsPage'
    | 'BranchManagerAccountsPage'
    | 'DelegateHomePage'
    | 'DelegateSettingsPage'
    | 'BranchCustomersPage'
    | 'BranchPaymentsPage'
    | null
  defaultQuery?: Record<string, string>
  notes?: string
}

export interface ManagementSectionDefinition {
  id: ManagementSectionId
  labelKey: string
  displayOrder: number
  requirement: AccessRequirement
}

/** إجراءات المهام — ليست شرط دخول التبويب (دخول القائمة = Task.Read فقط) */
export const TASK_ACTION_PERMISSIONS: readonly [
  PermissionRef,
  ...PermissionRef[],
] = [
  { module: 'Task', action: 'Create' },
  { module: 'Task', action: 'Assign' },
  { module: 'Task', action: 'Accept' },
  { module: 'Task', action: 'Reject' },
  { module: 'Task', action: 'Complete' },
  { module: 'Task', action: 'Confirm' },
  { module: 'Task', action: 'Transfer' },
]

/** @deprecated استخدم Task.Read لدخول التبويب؛ الإبقاء للتوافق مع اختبارات قديمة */
export const TASK_ANY_PERMISSIONS: readonly [PermissionRef, ...PermissionRef[]] =
  [{ module: 'Task', action: 'Read' }, ...TASK_ACTION_PERMISSIONS]

const TASK_READ: AccessRequirement = {
  type: 'single',
  permission: { module: 'Task', action: 'Read' },
}

const DELEGATE_FOLLOWUP_READ: AccessRequirement = {
  type: 'single',
  permission: { module: 'DelegateFollowup', action: 'Read' },
}

const MANAGEMENT_SUMMARY_READ: AccessRequirement = {
  type: 'single',
  permission: { module: 'ManagementSummary', action: 'Read' },
}

const MANAGEMENT_RATINGS_READ: AccessRequirement = {
  type: 'single',
  permission: { module: 'ManagementRatings', action: 'Read' },
}

/** دليل المتابعين أو أي صلاحية قسم منفّذ يحتاج الدليل (أقسام 06–13 لاحقًا). */
const FOLLOWER_WORK_DIRECTORY: AccessRequirement = {
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
}

const OVERSIGHT_SECTION_ACCESS: AccessRequirement = {
  type: 'anyOf',
  permissions: [
    { module: 'DelegateFollowup', action: 'Read' },
    { module: 'ManagementSummary', action: 'Read' },
    { module: 'ManagementRatings', action: 'Read' },
    { module: 'FollowerWork', action: 'Read' },
    { module: 'FollowerSalesReview', action: 'Read' },
  ],
}

export const MANAGEMENT_SECTIONS: readonly ManagementSectionDefinition[] = [
  {
    id: 'oversight',
    labelKey: 'sections.oversight',
    displayOrder: 5,
    requirement: OVERSIGHT_SECTION_ACCESS,
  },
  {
    /** alias: director → نفس مجموعة الإشراف؛ المسارات القديمة تبقى */
    id: 'director',
    labelKey: 'sections.oversight',
    displayOrder: 6,
    requirement: OVERSIGHT_SECTION_ACCESS,
  },
  {
    id: 'branch',
    labelKey: 'sections.branch',
    displayOrder: 10,
    requirement: {
      type: 'anyOf',
      permissions: [
        { module: 'Customer', action: 'Read' },
        { module: 'OrderList', action: 'Read' },
        { module: 'Order', action: 'Read' },
        ...SALES_REQUEST_TAB_PERMISSIONS,
        { module: 'SalesFollowup', action: 'Read' },
        { module: 'User', action: 'Read' },
        { module: 'ActivityLog', action: 'Read' },
        { module: 'Task', action: 'Read' },
      ],
    },
  },
  {
    id: 'delegate',
    labelKey: 'sections.delegate',
    displayOrder: 20,
    requirement: {
      type: 'anyOf',
      permissions: [
        { module: 'SalesRequest', action: 'TabExceptions' },
        { module: 'SalesRequest', action: 'ExceptionDecide' },
        { module: 'SalesFollowup', action: 'Read' },
        { module: 'User', action: 'ManagePermissions' },
        { module: 'Task', action: 'Read' },
      ],
    },
  },
] as const

export const MANAGEMENT_TABS: readonly ManagementTabDefinition[] = [
  // —— أعمال الفرع ——
  {
    id: 'customers',
    sectionId: 'branch',
    labelKey: 'tabs.customers',
    tabPath: 'customers',
    routeName: 'ManagementBranchCustomersRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Customer', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchCustomersPage',
    notes: 'بحث BM ≥2 أحرف ضمن الفرع — ليست CustomerView العامة ولا إنشاء',
  },
  {
    id: 'payments',
    sectionId: 'branch',
    labelKey: 'tabs.payments',
    tabPath: 'payments',
    routeName: 'ManagementBranchPaymentsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'OrderList', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchPaymentsPage',
    notes: 'ملخص تسديدات قائمة+فترة — قراءة فقط؛ ليست PaymentListPage بإنشاء تسديد',
  },
  {
    id: 'sales',
    sectionId: 'branch',
    labelKey: 'tabs.sales',
    tabPath: 'sales',
    routeName: 'ManagementBranchSalesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'BranchNewSale', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchNewSalesPage',
    notes:
      'صندوق NewSales: Workflow=Sold؛ قراءة مشتركة BranchManagerReadAtUtc؛ ليست SaleList',
  },
  {
    id: 'wrongSalesBranch',
    sectionId: 'branch',
    labelKey: 'tabs.wrongSales',
    tabPath: 'wrong-sales',
    routeName: 'ManagementBranchWrongSalesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'WrongSale', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'WrongSalesPage',
    defaultQuery: { mailbox: 'unreviewed' },
    notes: 'صندوق مصادقة ≤6 — مراجعة إدارة الفرع؛ ليس استثناء Hold',
  },
  {
    id: 'saleRequests',
    sectionId: 'branch',
    labelKey: 'tabs.saleRequests',
    tabPath: 'sale-requests',
    routeName: 'ManagementBranchSaleRequestsRoute',
    requirement: {
      type: 'anyOf',
      permissions: SALES_REQUEST_TAB_PERMISSIONS,
    },
    implementationStatus: 'ready',
    reuse: 'SalesRequestsWorkspace',
    notes: 'Source: SalesRequests/router.ts',
  },
  {
    id: 'saleFollowup',
    sectionId: 'branch',
    labelKey: 'tabs.saleFollowup',
    tabPath: 'sale-followup',
    routeName: 'ManagementBranchSaleFollowupRoute',
    requirement: {
      type: 'single',
      permission: { module: 'SalesFollowup', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'SalesFollowupPage',
    notes: 'متابعة موظفي المبيعات — ليست متابعة المندوبين',
  },
  {
    id: 'users',
    sectionId: 'branch',
    labelKey: 'tabs.users',
    tabPath: 'users',
    routeName: 'ManagementBranchUsersRoute',
    requirement: {
      type: 'single',
      permission: { module: 'User', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'UserListPage',
    notes: 'Source: User/router.ts User.Read',
  },
  {
    id: 'activityLog',
    sectionId: 'branch',
    labelKey: 'tabs.activityLog',
    tabPath: 'activity-log',
    routeName: 'ManagementBranchActivityLogRoute',
    requirement: {
      type: 'single',
      permission: { module: 'ActivityLog', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'ActivityLogListPage',
    notes: 'سجل نشاط عام — ليس سجل قرارات قانونية',
  },
  {
    id: 'branchAssignedTasks',
    sectionId: 'branch',
    labelKey: 'tabs.branchAssignedTasks',
    tabPath: 'assigned-tasks',
    routeName: 'ManagementBranchAssignedTasksRoute',
    requirement: TASK_READ,
    implementationStatus: 'ready',
    reuse: 'ManagementTasksPage',
    notes: 'مهامي — دخول القائمة بـ Task.Read؛ الإجراءات داخل الصفحة',
  },

  // —— أعمال المفوض ——
  {
    id: 'exceptions',
    sectionId: 'delegate',
    labelKey: 'tabs.exceptions',
    tabPath: 'exceptions',
    routeName: 'ManagementDelegateExceptionsRoute',
    requirement: { type: 'single', permission: { module: 'SalesRequest', action: 'TabExceptions' } },
    implementationStatus: 'ready',
    reuse: 'SalesRequestsWorkspace',
    defaultQuery: { tab: 'exceptions' },
    notes: 'استثناءات قائمة — قرار الخلفية يمنع موافقة المقدم على طلبه',
  },
  {
    id: 'salesEmployeeRating',
    sectionId: 'delegate',
    labelKey: 'tabs.salesEmployeeRating',
    tabPath: 'sales-employee-rating',
    routeName: 'ManagementDelegateSalesEmployeeRatingRoute',
    requirement: {
      type: 'single',
      permission: { module: 'SalesFollowup', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'SalesFollowupPage',
    notes: 'SalesFollowup — ليست DelegateFollowup',
  },
  {
    id: 'accounts',
    sectionId: 'delegate',
    labelKey: 'tabs.accounts',
    tabPath: 'accounts',
    routeName: 'ManagementDelegateAccountsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'BranchManagerAccount', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchManagerAccountsPage',
    notes:
      'حسابات مدير الفرع ضمن نطاق المفوض فقط — BranchManagerAccount.* بلا ManagePermissions',
  },
  {
    id: 'branchDeclining',
    sectionId: 'branch',
    labelKey: 'tabs.branchDeclining',
    tabPath: 'declining',
    routeName: 'ManagementBranchDecliningRoute',
    requirement: {
      type: 'single',
      permission: { module: 'BranchDeclining', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchDecliningPage',
    notes: 'ممتاز→سيء/متلكئ خلال 15 يومًا — ليست FollowerDeclining',
  },
  {
    id: 'branchRatings',
    sectionId: 'branch',
    labelKey: 'tabs.branchRatings',
    tabPath: 'ratings',
    routeName: 'ManagementBranchRatingsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'ManagementRatings', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'ManagementRatingsPage',
    notes: 'لوحة تقييمات الفرع (StatsPage المصدر) — مقاييس ManagementRatings بنطاق الفرع',
  },
  {
    id: 'branchVisitWorkspace',
    sectionId: 'branch',
    labelKey: 'tabs.branchVisitWorkspace',
    tabPath: 'visit-workspace',
    routeName: 'ManagementBranchVisitWorkspaceRoute',
    requirement: {
      type: 'single',
      permission: { module: 'BranchVisit', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchVisitsPage',
    notes: 'دورة فتح/تقييم/مغادرة لمدير الفرع — ليست تبويب المصدر المستقل لكنها واجهة الويب اللازمة',
  },
  {
    id: 'delegateHome',
    sectionId: 'delegate',
    labelKey: 'tabs.delegateHome',
    tabPath: 'home',
    routeName: 'ManagementDelegateHomeRoute',
    requirement: {
      type: 'single',
      permission: { module: 'DelegateHome', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'DelegateHomePage',
    notes: 'مؤشرات الشكاوى/الاستثناءات/WrongSale ضمن الصلاحية والنطاق',
  },
  {
    id: 'delegateRatings',
    sectionId: 'delegate',
    labelKey: 'tabs.delegateRatings',
    tabPath: 'ratings',
    routeName: 'ManagementDelegateRatingsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'ManagementRatings', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'ManagementRatingsPage',
    notes: 'تقييمات المفوض بنفس منطق StatsPage المصدر — ليست SalesFollowup',
  },
  {
    id: 'delegateSettings',
    sectionId: 'delegate',
    labelKey: 'tabs.delegateSettings',
    tabPath: 'settings',
    routeName: 'ManagementDelegateSettingsRoute',
    requirement: { type: 'publicAuthenticated' },
    implementationStatus: 'ready',
    reuse: 'DelegateSettingsPage',
    notes: 'جلسة + رابط تعليمي — ليست إعدادات مركزية وهمية',
  },
  {
    id: 'wrongSalesDelegate',
    sectionId: 'delegate',
    labelKey: 'tabs.wrongSales',
    tabPath: 'wrong-sales',
    routeName: 'ManagementDelegateWrongSalesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'WrongSale', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'WrongSalesPage',
    defaultQuery: { mailbox: 'unread' },
    notes: 'صندوق المفوض للمبيعات الخاطئة — تعليم مقروء بلا تغيير حالة البيع',
  },
  {
    id: 'assignedTasks',
    sectionId: 'delegate',
    labelKey: 'tabs.assignedTasks',
    tabPath: 'assigned-tasks',
    routeName: 'ManagementDelegateAssignedTasksRoute',
    requirement: TASK_READ,
    implementationStatus: 'ready',
    reuse: 'ManagementTasksPage',
    notes: 'مهامي للمفوض — دخول بـ Task.Read؛ ليست عمل المتابعين',
  },

  // —— الإشراف والمتابعة (كان يُسمّى مديرًا بالخطأ) ——
  {
    id: 'delegateFollowup',
    sectionId: 'director',
    labelKey: 'tabs.delegateFollowup',
    tabPath: 'delegate-followup',
    routeName: 'ManagementDirectorDelegateFollowupRoute',
    requirement: DELEGATE_FOLLOWUP_READ,
    implementationStatus: 'ready',
    reuse: 'DelegateFollowupPage',
    notes:
      'متابعة المندوبين — Read فقط لدخول التبويب؛ UpsertBoardRating إجراء داخل الصفحة',
  },

  {
    id: 'summary',
    sectionId: 'oversight',
    labelKey: 'tabs.summary',
    tabPath: 'summary',
    routeName: 'ManagementSummaryRoute',
    requirement: MANAGEMENT_SUMMARY_READ,
    implementationStatus: 'ready',
    reuse: 'ManagementSummaryPage',
    notes:
      'الملخص الإداري (مصفوفة المدير) — Read للتبويب؛ Drilldown للتفاصيل؛ ليس ملخص المبيعات',
  },

  {
    id: 'ratings',
    sectionId: 'oversight',
    labelKey: 'tabs.ratings',
    tabPath: 'ratings',
    routeName: 'ManagementRatingsRoute',
    requirement: MANAGEMENT_RATINGS_READ,
    implementationStatus: 'ready',
    reuse: 'ManagementRatingsPage',
    notes:
      'التقييمات العامة — Read للتبويب؛ DetailsRead لزبائن الفئات؛ ليست SalesFollowup',
  },

  {
    id: 'followerWork',
    sectionId: 'oversight',
    labelKey: 'tabs.followerWork',
    tabPath: 'follower-work',
    routeName: 'ManagementFollowerWorkRoute',
    requirement: FOLLOWER_WORK_DIRECTORY,
    implementationStatus: 'ready',
    reuse: 'FollowerWorkPage',
    notes:
      'هيكل عمل المتابعين + بطاقات — أقسام 06–13 ready',
  },

  // —— أقسام عمل المتابعين (ready تُدرج في المسارات؛ الباقي موثّق غير منفَّذ) ——
  {
    id: 'followerSales',
    sectionId: 'oversight',
    labelKey: 'tabs.followerSales',
    tabPath: 'follower-work/sales',
    routeName: 'ManagementFollowerSalesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerSalesReview', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerSalesReviewPage',
    notes:
      'مصادقة البيع — قراءة فقط من اللوحة؛ الكتابة من تطبيق اللجنة FollowerSalesReview.Authenticate',
  },
  {
    id: 'followerDeclining',
    sectionId: 'oversight',
    labelKey: 'tabs.followerDeclining',
    tabPath: 'follower-work/declining',
    routeName: 'ManagementFollowerDecliningRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerDeclining', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerDecliningPage',
    notes:
      'المتدنية — هبوط درجتين خلال 7 أيام من لقطات التقييم؛ قراءة فقط FollowerDeclining.Read',
  },
  {
    id: 'followerNotes',
    sectionId: 'oversight',
    labelKey: 'tabs.followerNotes',
    tabPath: 'follower-work/notes',
    routeName: 'ManagementFollowerNotesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerNotes', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerNotesPage',
    notes: 'ملاحظات الزبون — قراءة فقط؛ الكتابة من تطبيق المتابع',
  },
  {
    id: 'followerOldCustomers',
    sectionId: 'oversight',
    labelKey: 'tabs.followerOldCustomers',
    tabPath: 'follower-work/old-customers',
    routeName: 'ManagementFollowerOldCustomersRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerOldCustomers', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerOldCustomersPage',
    notes: 'LegalFollowupCases؛ الإلغاء بصلاحية LegalFollowup.Cancel',
  },
  {
    id: 'followerDelegateNotes',
    sectionId: 'oversight',
    labelKey: 'tabs.followerDelegateNotes',
    tabPath: 'follower-work/delegate-notes',
    routeName: 'ManagementFollowerDelegateNotesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerDelegateNotes', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerDelegateNotesPage',
    notes: 'FollowerDelegateNotes — قراءة لوحة؛ كتابة تطبيق المتابع',
  },
  {
    id: 'followerCollection',
    sectionId: 'oversight',
    labelKey: 'tabs.followerCollection',
    tabPath: 'follower-work/collection',
    routeName: 'ManagementFollowerCollectionRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerCollection', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerCollectionPage',
    notes: 'قراءة مستقلة؛ ManageEnabled منفصل لتبديل التمكين',
  },
  {
    id: 'followerLiveMap',
    sectionId: 'oversight',
    labelKey: 'tabs.followerLiveMap',
    tabPath: 'follower-work/live-map',
    routeName: 'ManagementFollowerLiveMapRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerLocation', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerLiveMapPage',
    notes: 'SalesLocationPoints للمتابعين (Motaba) + دوام؛ poll 15s',
  },
  {
    id: 'followerRoutes',
    sectionId: 'oversight',
    labelKey: 'tabs.followerRoutes',
    tabPath: 'follower-work/routes',
    routeName: 'ManagementFollowerRoutesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'FollowerRoute', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'FollowerRoutesPage',
    notes: 'مسار يوم بغداد — markers بلا polyline مضلل',
  },
  {
    id: 'committeeTasks',
    sectionId: 'oversight',
    labelKey: 'tabs.committeeTasks',
    tabPath: 'committee-tasks',
    routeName: 'ManagementCommitteeTasksRoute',
    requirement: {
      type: 'single',
      permission: { module: 'CommitteeTask', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'CommitteeTasksPage',
    notes: 'نشرة مهام اللجنة — ليست ManagementTasks؛ CRUD للوحة + AppRead للتطبيق',
  },
  {
    id: 'appNotes',
    sectionId: 'oversight',
    labelKey: 'tabs.appNotes',
    tabPath: 'app-notes',
    routeName: 'ManagementAppNotesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'AppNote', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'AppNotesPage',
    notes: 'وارد ملاحظات التطبيقات — Unread/Read فقط؛ Create من التطبيقات',
  },
  {
    id: 'waivers',
    sectionId: 'oversight',
    labelKey: 'tabs.waivers',
    tabPath: 'waivers',
    routeName: 'ManagementWaiversRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Waiver', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'WaiversPage',
    notes: 'قراءة إشراف لكل الطلبات؛ القرار Apply/Decide بصلاحيات منفصلة',
  },
  {
    id: 'branchWaivers',
    sectionId: 'branch',
    labelKey: 'tabs.branchWaivers',
    tabPath: 'waivers',
    routeName: 'ManagementBranchWaiversRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Waiver', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'WaiversPage',
    defaultQuery: { scope: 'branch' },
    notes: 'إنشاء ومتابعة طلبات تنازل الفرع',
  },
  {
    id: 'delegateWaivers',
    sectionId: 'delegate',
    labelKey: 'tabs.delegateWaivers',
    tabPath: 'waivers',
    routeName: 'ManagementDelegateWaiversRoute',
    requirement: {
      type: 'anyOf',
      permissions: [
        { module: 'Waiver', action: 'Read' },
        { module: 'Waiver', action: 'Decide' },
      ],
    },
    implementationStatus: 'ready',
    reuse: 'WaiversPage',
    defaultQuery: { scope: 'delegate' },
    notes: 'صندوق المفوض — قرار قبول/رفض',
  },
  {
    id: 'legalNames',
    sectionId: 'branch',
    labelKey: 'tabs.legalNames',
    tabPath: 'legal-names',
    routeName: 'ManagementLegalNamesRoute',
    requirement: {
      type: 'single',
      permission: { module: 'LegalNames', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'LegalNamesPage',
    notes: 'طابور ضعاف الدفع — قرارات فرع؛ ليست LegalFollowup',
  },
  {
    id: 'priceBoard',
    sectionId: 'oversight',
    labelKey: 'tabs.priceBoard',
    tabPath: 'price-board',
    routeName: 'ManagementPriceBoardRoute',
    requirement: {
      type: 'single',
      permission: { module: 'PriceBoard', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'PriceBoardPage',
    notes: 'إعلانات أسعار محافظة — معلوماتية؛ ليست أسعار Product',
  },
  {
    id: 'announcements',
    sectionId: 'oversight',
    labelKey: 'tabs.announcements',
    tabPath: 'announcements',
    routeName: 'ManagementAnnouncementsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Announcement', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'AnnouncementsPage',
    notes: 'بانر تطبيقات؛ ليست AppNote ولا Push',
  },
  {
    id: 'tutorials',
    sectionId: 'oversight',
    labelKey: 'tabs.tutorials',
    tabPath: 'tutorials',
    routeName: 'ManagementTutorialsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Tutorial', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'TutorialVideosPage',
    notes: 'أسئلة + روابط خارجية؛ جمهور delegate/branch-manager/follower',
  },
  {
    id: 'complaints',
    sectionId: 'delegate',
    labelKey: 'tabs.complaints',
    tabPath: 'complaints',
    routeName: 'ManagementDelegateComplaintsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'Complaint', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'ComplaintsPage',
    notes: 'صندوق ComplaintMessage — كاتب اللجنة يظهر للمستلم المخول',
  },
  {
    id: 'branchVisits',
    sectionId: 'delegate',
    labelKey: 'tabs.branchVisits',
    tabPath: 'branch-visits',
    routeName: 'ManagementDelegateBranchVisitsRoute',
    requirement: {
      type: 'single',
      permission: { module: 'BranchVisit', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'BranchVisitsPage',
    notes: 'سجل زيارات الفرع للمفوض — Open/Rate/Depart لمالك الزيارة ضمن النطاق',
  },
  {
    id: 'legalDecisions',
    sectionId: 'branch',
    labelKey: 'tabs.legalDecisions',
    tabPath: 'decision-history',
    routeName: 'ManagementBranchDecisionHistoryRoute',
    requirement: {
      type: 'single',
      permission: { module: 'CustomerDecision', action: 'Read' },
    },
    implementationStatus: 'ready',
    reuse: 'CustomerDecisionsPage',
    notes: 'سجل قرارات الفرع — ليس ActivityLog',
  },
  {
    id: 'committeeRatings',
    sectionId: 'delegate',
    labelKey: 'tabs.committeeRatings',
    tabPath: 'committee-ratings',
    routeName: 'ManagementDelegateCommitteeRatingsRoute',
    requirement: { type: 'publicAuthenticated' },
    implementationStatus: 'not_implemented',
    reuse: null,
  },
] as const

export function getSectionById(
  id: string
): ManagementSectionDefinition | undefined {
  return MANAGEMENT_SECTIONS.find((s) => s.id === id)
}

export function getTabByIds(
  sectionId: string,
  tabId: string
): ManagementTabDefinition | undefined {
  const normalizedSection =
    sectionId === 'oversight' ? 'director' : sectionId
  return MANAGEMENT_TABS.find(
    (t) =>
      (t.sectionId === sectionId ||
        (sectionId === 'oversight' && t.sectionId === 'director') ||
        (sectionId === 'director' && t.sectionId === 'oversight') ||
        t.sectionId === normalizedSection) &&
      (t.id === tabId || t.tabPath === tabId)
  )
}

export function getReadyTabsForSection(
  sectionId: ManagementSectionId
): ManagementTabDefinition[] {
  const aliases =
    sectionId === 'oversight' || sectionId === 'director'
      ? new Set(['oversight', 'director'])
      : new Set([sectionId])
  return MANAGEMENT_TABS.filter(
    (t) =>
      aliases.has(t.sectionId) &&
      (t.implementationStatus === 'ready' ||
        t.implementationStatus === 'partial')
  )
}

export function getReadyManagementTabs(): ManagementTabDefinition[] {
  return MANAGEMENT_TABS.filter(
    (t) =>
      t.implementationStatus === 'ready' ||
      t.implementationStatus === 'partial'
  )
}

/** أقسام ظاهرة في شريط الأقسام — بدون تكرار alias director/oversight */
export function getVisibleManagementSections(): ManagementSectionDefinition[] {
  return MANAGEMENT_SECTIONS.filter((s) => s.id !== 'director')
}
