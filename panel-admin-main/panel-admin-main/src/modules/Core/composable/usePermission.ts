import { computed } from 'vue'
import { useAuthStore } from '@/modules/Auth/store'
import type {
  PermissionAction,
  PermissionModule,
} from '@/modules/Auth/types/model'
import type { AccessRequirement } from '@/modules/Core/access/access.types'
import {
  canAccess,
  evaluateAccess,
} from '@/modules/Core/access/evaluateAccess'

/**
 * ربط تفاعلي بين صلاحيات Pinia ودوال التقييم النقية.
 * لا يفك JWT ولا يعتمد على localStorage كإثبات جاهزية.
 */
export function usePermission() {
  const authStore = useAuthStore()

  const permissionsStatus = computed(() => authStore.permissionsStatus)

  const permissionsReady = computed(
    () => authStore.isAuthenticated && authStore.permissionsStatus === 'ready'
  )

  function can(module: PermissionModule, action: PermissionAction): boolean {
    return authStore.hasPermission(module, action)
  }

  function canAny(
    checks: Array<{ module: PermissionModule; action: PermissionAction }>
  ): boolean {
    return authStore.hasAnyPermission(checks)
  }

  function canAll(
    checks: Array<{ module: PermissionModule; action: PermissionAction }>
  ): boolean {
    return authStore.hasAllPermissions(checks)
  }

  function decide(requirement: AccessRequirement | null | undefined) {
    return evaluateAccess({
      permissionsReady: permissionsReady.value,
      isAuthenticated: authStore.isAuthenticated,
      hasPermission: authStore.hasPermission,
      requirement,
    })
  }

  function allows(requirement: AccessRequirement | null | undefined): boolean {
    return canAccess({
      permissionsReady: permissionsReady.value,
      isAuthenticated: authStore.isAuthenticated,
      hasPermission: authStore.hasPermission,
      requirement,
    })
  }

  return {
    permissionsStatus,
    permissionsReady,
    can,
    canAny,
    canAll,
    decide,
    allows,
  }
}
