import { useAuthStore } from '@/modules/Auth/store'
import type {
  PermissionModule,
  PermissionAction,
} from '@/modules/Auth/types/model'

export function usePermission() {
  const authStore = useAuthStore()

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

  return {
    can,
    canAny,
    canAll,
  }
}
