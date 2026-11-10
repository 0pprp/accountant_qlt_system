import type { PermissionRef } from '@/modules/Core/access/access.types'

/**
 * تبعيات صلاحيات معلنة لمحرر PowersForm — لا توسيع خفي عند التشغيل.
 * الإجراء لا يمنح Read ضمنيًا؛ التبعية تُعرض كتوضيح فقط.
 */
export interface PermissionDependency {
  action: PermissionRef
  requires: readonly PermissionRef[]
  noteAr: string
}

export const PERMISSION_DEPENDENCIES: readonly PermissionDependency[] = [
  {
    action: { module: 'DelegateFollowup', action: 'UpsertBoardRating' },
    requires: [{ module: 'DelegateFollowup', action: 'Read' }],
    noteAr: 'تقييم عامل اللوحة يتطلب قراءة متابعة المندوبين لفتح التبويب؛ لا يُمنح Read تلقائيًا.',
  },
  {
    action: { module: 'SalesFollowup', action: 'UpsertManagerRating' },
    requires: [{ module: 'SalesFollowup', action: 'Read' }],
    noteAr: 'تقييم المدير يتطلب قراءة متابعة المبيعات؛ لا يُمنح Read تلقائيًا.',
  },
  {
    action: { module: 'SalesFollowup', action: 'ManageAppBlock' },
    requires: [{ module: 'SalesFollowup', action: 'Read' }],
    noteAr: 'إيقاف التطبيق إجراء منفصل عن قراءة اللوحة.',
  },
  {
    action: { module: 'Task', action: 'Create' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'إنشاء مهمة لا يفتح قائمة المهام الإدارية بدون Task.Read.',
  },
  {
    action: { module: 'Task', action: 'Assign' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'الإسناد لا يمنح قراءة القائمة.',
  },
  {
    action: { module: 'Task', action: 'Transfer' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'النقل لا يمنح قراءة القائمة.',
  },
  {
    action: { module: 'Task', action: 'Accept' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'قبول المهمة إجراء؛ دخول التبويب يبقى Task.Read.',
  },
  {
    action: { module: 'Task', action: 'Reject' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'رفض المهمة إجراء؛ دخول التبويب يبقى Task.Read.',
  },
  {
    action: { module: 'Task', action: 'Complete' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'إنجاز المهمة إجراء؛ دخول التبويب يبقى Task.Read.',
  },
  {
    action: { module: 'Task', action: 'Confirm' },
    requires: [{ module: 'Task', action: 'Read' }],
    noteAr: 'التصديق إجراء؛ دخول التبويب يبقى Task.Read.',
  },
  {
    action: { module: 'User', action: 'ManagePermissions' },
    requires: [{ module: 'User', action: 'Read' }],
    noteAr: 'إدارة الصلاحيات تتطلب قراءة المستخدمين؛ لا تُمنح تلقائيًا لقوالب المدير/المراقب.',
  },
]

export function dependenciesFor(
  module: string,
  action: string
): PermissionDependency | undefined {
  return PERMISSION_DEPENDENCIES.find(
    (d) => d.action.module === module && d.action.action === action
  )
}
