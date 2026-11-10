import type { RouteLocationNormalized } from 'vue-router'
import type { AccessRequirement } from './access.types'
import { evaluateAccess } from './evaluateAccess'
import { getNavigationSectionByRouteName } from '@/modules/Core/navigation/navigation.registry'
import type {
  PermissionAction,
  PermissionModule,
} from '@/modules/Auth/types/model'
import type { PermissionsLifecycleStatus } from '@/modules/Auth/session/session.types'

/** مسارات عامة مصرّح بها صراحةً — لا حماية أعمال. */
export const EXPLICIT_PUBLIC_ROUTE_NAMES = new Set([
  'AuthLoginRoute',
  'SessionBootRoute',
  'NotFoundRoute',
  // مسجّل في DEV فقط عبر Core/router — لا يظهر في إنتاج.
  'UiKitPreviewRoute',
])

/**
 * مسارات مصادَق عليها بلا شرط مورد — معلنة صراحةً.
 * AccessDenied: غلاف الجلسة.
 */
export const AUTHENTICATED_SHELL_ROUTE_NAMES = new Set([
  'AccessDeniedRoute',
])

export type RouteAccessOutcome =
  | { type: 'allow' }
  | { type: 'login' }
  | { type: 'boot'; remember: boolean }
  | { type: 'denied'; from?: string }
  | { type: 'home' }

export interface ResolveRouteAccessInput {
  to: Pick<RouteLocationNormalized, 'name' | 'meta' | 'matched' | 'fullPath'>
  isAuthenticated: boolean
  permissionsStatus: PermissionsLifecycleStatus
  hasPermission: (module: PermissionModule, action: PermissionAction) => boolean
}

function permissionFromMeta(
  meta: Record<string, unknown> | undefined
): AccessRequirement | null {
  const accessRequirement = meta?.accessRequirement as
    | AccessRequirement
    | undefined
  if (accessRequirement?.type) {
    return accessRequirement
  }

  const permission = meta?.permission as
    | { module?: string; action?: string }
    | undefined
  if (permission?.module && permission?.action) {
    return {
      type: 'single',
      permission: {
        module: permission.module as PermissionModule,
        action: permission.action as PermissionAction,
      },
    }
  }
  return null
}

/**
 * يستخرج شرط الوصول من المسار المطابق (بما فيه الفرعي) أو سجل التنقل.
 * المسار غير المعلن لا يُعامل كعام ضمنيًا.
 */
export function resolveRequirementForRoute(
  to: Pick<RouteLocationNormalized, 'name' | 'meta' | 'matched'>
): {
  requirement: AccessRequirement | null
  isProtected: boolean
  isExplicitPublic: boolean
  isDeclared: boolean
} {
  const routeName = String(to.name ?? '')

  if (EXPLICIT_PUBLIC_ROUTE_NAMES.has(routeName)) {
    return {
      requirement: null,
      isProtected: false,
      isExplicitPublic: true,
      isDeclared: true,
    }
  }

  if (AUTHENTICATED_SHELL_ROUTE_NAMES.has(routeName)) {
    return {
      requirement: { type: 'publicAuthenticated' },
      isProtected: true,
      isExplicitPublic: false,
      isDeclared: true,
    }
  }

  const matched = [...(to.matched ?? [])].reverse()
  for (const record of matched) {
    const fromMeta = permissionFromMeta(record.meta as Record<string, unknown>)
    if (fromMeta) {
      return {
        requirement: fromMeta,
        isProtected: true,
        isExplicitPublic: false,
        isDeclared: true,
      }
    }
  }

  const fromMetaCurrent = permissionFromMeta(to.meta as Record<string, unknown>)
  if (fromMetaCurrent) {
    return {
      requirement: fromMetaCurrent,
      isProtected: true,
      isExplicitPublic: false,
      isDeclared: true,
    }
  }

  for (const record of matched) {
    const section = getNavigationSectionByRouteName(String(record.name ?? ''))
    if (section) {
      return {
        requirement: section.requirement,
        isProtected: true,
        isExplicitPublic: false,
        isDeclared: true,
      }
    }
  }

  const section = getNavigationSectionByRouteName(routeName)
  if (section) {
    return {
      requirement: section.requirement,
      isProtected: true,
      isExplicitPublic: false,
      isDeclared: true,
    }
  }

  const requiresAuthFlag = Boolean(
    to.meta?.requiresAuth ||
      matched.some((record) => Boolean(record.meta?.requiresAuth))
  )

  if (requiresAuthFlag) {
    return {
      requirement: null,
      isProtected: true,
      isExplicitPublic: false,
      isDeclared: true,
    }
  }

  // غير معلن: ليس عامًا وليس له شرط معروف
  return {
    requirement: null,
    isProtected: true,
    isExplicitPublic: false,
    isDeclared: false,
  }
}

export function resolveRouteAccess(
  input: ResolveRouteAccessInput
): RouteAccessOutcome {
  const routeName = String(input.to.name ?? '')
  const { requirement, isProtected, isExplicitPublic, isDeclared } =
    resolveRequirementForRoute(input.to)

  if (isExplicitPublic || EXPLICIT_PUBLIC_ROUTE_NAMES.has(routeName)) {
    if (
      input.isAuthenticated &&
      routeName === 'AuthLoginRoute' &&
      input.permissionsStatus === 'ready'
    ) {
      return { type: 'home' }
    }
    return { type: 'allow' }
  }

  if (!input.isAuthenticated) {
    return { type: 'login' }
  }

  if (input.permissionsStatus === 'loading') {
    if (routeName === 'SessionBootRoute') return { type: 'allow' }
    return { type: 'boot', remember: isDeclared && isProtected }
  }

  if (input.permissionsStatus === 'error') {
    if (routeName === 'SessionBootRoute') return { type: 'allow' }
    return { type: 'boot', remember: isDeclared && isProtected }
  }

  // ready — المسار غير المعلن بلا شرط لا يحصل على allow لمجرد التسجيل
  if (!isDeclared) {
    return { type: 'denied', from: routeName || 'undeclared' }
  }

  if (!isProtected) {
    return { type: 'allow' }
  }

  if (requirement == null) {
    return { type: 'denied', from: routeName }
  }

  const decision = evaluateAccess({
    permissionsReady: true,
    isAuthenticated: true,
    hasPermission: input.hasPermission,
    requirement,
  })

  if (decision === 'allowed') return { type: 'allow' }
  if (decision === 'loading') return { type: 'boot', remember: true }

  return { type: 'denied', from: routeName }
}
