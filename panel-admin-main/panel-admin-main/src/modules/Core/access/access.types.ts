import type {
  PermissionAction,
  PermissionModule,
} from '@/modules/Auth/types/model'

/** مرجع صلاحية واحدة كما يعيدها مسار المصادقة (Module + Action). */
export interface PermissionRef {
  module: PermissionModule
  action: PermissionAction
}

/**
 * شرط وصول صريح.
 * التركيبات المتناقضة ممنوعة على مستوى الأنواع (لا يمكن دمج single مع allOf في نفس الكائن).
 */
export type AccessRequirement =
  | { type: 'publicAuthenticated' }
  | { type: 'single'; permission: PermissionRef }
  | { type: 'allOf'; permissions: readonly [PermissionRef, ...PermissionRef[]] }
  | { type: 'anyOf'; permissions: readonly [PermissionRef, ...PermissionRef[]] }

export type AccessDecision = 'loading' | 'allowed' | 'denied'

export interface EvaluateAccessInput {
  /** هل اكتمل تحميل صلاحيات الجلسة؟ */
  permissionsReady: boolean
  isAuthenticated: boolean
  /** يتحقق من صلاحية واحدة ضد مخزن الجلسة */
  hasPermission: (module: PermissionModule, action: PermissionAction) => boolean
  requirement: AccessRequirement | null | undefined
}
