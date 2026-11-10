import type { PermissionAction } from '@/modules/Auth/types/model'

/** تبويبات سير العمل — مطابقة SalesRequestTabMap في الـAPI. */
export type WorkflowTabKey =
  | 'unassigned'
  | 'incoming'
  | 'prepared'
  | 'inspected'
  | 'pending'
  | 'rejected'
  | 'sold'

export type ExtraViewKey = 'all' | 'sent' | 'exceptions' | 'legal' | 'deleted'

export type SalesRequestTabKey = WorkflowTabKey | ExtraViewKey

export interface TabDefinition {
  key: SalesRequestTabKey
  label: string
  /** لون مؤشر الحالة */
  color: string
  group: 'workflow' | 'extra'
  /** صلاحية قراءة التبويب — null لـ «الكل» (اتحاد) أو عروض غير منفَّذة */
  permissionAction: PermissionAction | null
  implementationStatus: 'ready' | 'not_implemented'
}

export const WORKFLOW_TABS: readonly TabDefinition[] = [
  {
    key: 'unassigned',
    label: 'غير المسند',
    color: '#9A6700',
    group: 'workflow',
    permissionAction: 'ReadUnassigned',
    implementationStatus: 'ready',
  },
  {
    key: 'incoming',
    label: 'طلبات البيع',
    color: '#006A64',
    group: 'workflow',
    permissionAction: 'TabIncoming',
    implementationStatus: 'ready',
  },
  {
    key: 'prepared',
    label: 'جاهز للبيع',
    color: '#0B6E4F',
    group: 'workflow',
    permissionAction: 'TabPrepared',
    implementationStatus: 'ready',
  },
  {
    key: 'inspected',
    label: 'تم الكشف',
    color: '#1B4F72',
    group: 'workflow',
    permissionAction: 'TabInspected',
    implementationStatus: 'ready',
  },
  {
    key: 'pending',
    label: 'معلقة',
    color: '#7D6608',
    group: 'workflow',
    permissionAction: 'TabPending',
    implementationStatus: 'ready',
  },
  {
    key: 'rejected',
    label: 'مرفوض',
    color: '#922B21',
    group: 'workflow',
    permissionAction: 'TabRejected',
    implementationStatus: 'ready',
  },
  {
    key: 'sold',
    label: 'تم البيع',
    color: '#145A32',
    group: 'workflow',
    permissionAction: 'TabSold',
    implementationStatus: 'ready',
  },
] as const

/**
 * عروض إضافية — «الكل» اتحاد سير العمل فقط.
 * sent/exceptions/deleted/legal جاهزة؛ legal عبر LegalFollowupCases + TabLegal.
 * sent/exceptions متداخلة مع تبويبات سير العمل ولا تدخل في «الكل» ولا مجموع العدادات.
 */
export const EXTRA_VIEWS: readonly TabDefinition[] = [
  {
    key: 'all',
    label: 'الكل',
    color: '#34495E',
    group: 'extra',
    permissionAction: null,
    implementationStatus: 'ready',
  },
  {
    key: 'sent',
    label: 'الطلبات المرسلة',
    color: '#5D6D7E',
    group: 'extra',
    permissionAction: 'TabSent',
    implementationStatus: 'ready',
  },
  {
    key: 'exceptions',
    label: 'الاستثناءات',
    color: '#AF601A',
    group: 'extra',
    permissionAction: 'TabExceptions',
    implementationStatus: 'ready',
  },
  {
    key: 'legal',
    label: 'القانونية',
    color: '#4A235A',
    group: 'extra',
    permissionAction: 'TabLegal',
    implementationStatus: 'ready',
  },
  {
    key: 'deleted',
    label: 'المحذوفة',
    color: '#566573',
    group: 'extra',
    permissionAction: 'TabDeleted',
    implementationStatus: 'ready',
  },
] as const

export const WORKFLOW_PERMISSION_ACTIONS: readonly PermissionAction[] = [
  'ReadUnassigned',
  'TabIncoming',
  'TabPrepared',
  'TabInspected',
  'TabPending',
  'TabRejected',
  'TabSold',
]

export const EXTRA_VIEW_PERMISSION_ACTIONS: readonly PermissionAction[] = [
  'TabSent',
  'TabExceptions',
  'TabDeleted',
  'TabLegal',
]

/** كل صلاحيات التبويبات — أساس anyOf لفتح الحاوية والتنقل. */
export const TAB_PERMISSION_ACTIONS: readonly PermissionAction[] = [
  ...WORKFLOW_PERMISSION_ACTIONS,
  ...EXTRA_VIEW_PERMISSION_ACTIONS,
]

/** عروض متداخلة: صف الطلب قد يظهر فيها وفي تبويب سير العمل معًا. */
export const OVERLAPPING_VIEW_KEYS: readonly string[] = ['sent', 'exceptions']

export function isOverlappingView(key: string): boolean {
  return OVERLAPPING_VIEW_KEYS.includes(key)
}

export function tabByKey(key: string): TabDefinition | undefined {
  return (
    WORKFLOW_TABS.find((t) => t.key === key) ??
    EXTRA_VIEWS.find((t) => t.key === key)
  )
}

export function isWorkflowTab(key: string): key is WorkflowTabKey {
  return WORKFLOW_TABS.some((t) => t.key === key)
}
