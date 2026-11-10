import type {
  AccessDecision,
  AccessRequirement,
  EvaluateAccessInput,
  PermissionRef,
} from './access.types'

function isValidPermissionRef(value: unknown): value is PermissionRef {
  if (!value || typeof value !== 'object') return false
  const ref = value as PermissionRef
  return (
    typeof ref.module === 'string' &&
    ref.module.length > 0 &&
    typeof ref.action === 'string' &&
    ref.action.length > 0
  )
}

function isValidRequirement(
  requirement: AccessRequirement | null | undefined
): requirement is AccessRequirement {
  if (!requirement || typeof requirement !== 'object') return false

  switch (requirement.type) {
    case 'publicAuthenticated':
      return true
    case 'single':
      return isValidPermissionRef(requirement.permission)
    case 'allOf':
    case 'anyOf':
      return (
        Array.isArray(requirement.permissions) &&
        requirement.permissions.length > 0 &&
        requirement.permissions.every(isValidPermissionRef)
      )
    default:
      return false
  }
}

/**
 * تقييم نقي لشرط الوصول.
 * - أثناء التحميل: لا سماح ولا رفض نهائي.
 * - شرط مفقود/غير صالح لقسم محمي: رفض (لا سماح ضمني).
 */
export function evaluateAccess(input: EvaluateAccessInput): AccessDecision {
  if (!input.permissionsReady) return 'loading'
  if (!input.isAuthenticated) return 'denied'

  const { requirement } = input

  // قسم محمي بلا شرط صريح = رفض (يمنع السماح بالغلط)
  if (requirement == null) return 'denied'

  if (!isValidRequirement(requirement)) return 'denied'

  switch (requirement.type) {
    case 'publicAuthenticated':
      return 'allowed'
    case 'single':
      return input.hasPermission(
        requirement.permission.module,
        requirement.permission.action
      )
        ? 'allowed'
        : 'denied'
    case 'allOf':
      return requirement.permissions.every((p) =>
        input.hasPermission(p.module, p.action)
      )
        ? 'allowed'
        : 'denied'
    case 'anyOf':
      return requirement.permissions.some((p) =>
        input.hasPermission(p.module, p.action)
      )
        ? 'allowed'
        : 'denied'
    default:
      return 'denied'
  }
}

export function canAccess(input: EvaluateAccessInput): boolean {
  return evaluateAccess(input) === 'allowed'
}
