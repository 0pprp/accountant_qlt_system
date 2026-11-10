import type { NavigationGuardNext, RouteLocationNormalized } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store/index'
import { resolveRouteAccess } from '@/modules/Core/access/resolveRouteAccess'
import { rememberIntendedNavigation } from '@/modules/Auth/session/intendedNavigation'
import { bootstrapSession } from '@/modules/Auth/session/bootstrapSession'

let hydrateStarted = false

/**
 * يبدأ التحقق فقط عندما تكون الجلسة بانتظار التهيئة (بعد إماهة بلا ثقة)،
 * ولا يعيد reclaim لجلسة جاهزة من login في الذاكرة.
 */
function ensureBootstrap(authStore: ReturnType<typeof useAuthStore>) {
  if (hydrateStarted) return
  if (!authStore.refreshToken) return

  hydrateStarted = true

  if (authStore.permissionsStatus === 'loading') {
    void bootstrapSession()
    return
  }

  if (
    authStore.isAuthenticated &&
    authStore.permissionsStatus === 'ready' &&
    authStore.convertedPermissions != null
  ) {
    // جلسة موثوقة من دخول ناجح في هذه العملية
    return
  }

  authStore.reclaimPersistedSession()
  void bootstrapSession()
}

export function resetGuardBootstrapFlagForTests() {
  hydrateStarted = false
}

export function authGuard(
  to: RouteLocationNormalized,
  _from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  const authStore = useAuthStore()
  ensureBootstrap(authStore)

  const outcome = resolveRouteAccess({
    to,
    isAuthenticated: authStore.isAuthenticated,
    permissionsStatus: authStore.permissionsStatus,
    hasPermission: authStore.hasPermission,
  })

  switch (outcome.type) {
    case 'allow':
      next()
      return
    case 'login':
      next({ name: 'AuthLoginRoute' })
      return
    case 'home':
      next({ name: 'HomeMainRoute' })
      return
    case 'boot':
      if (outcome.remember) {
        rememberIntendedNavigation(to)
      }
      if (to.name === 'SessionBootRoute') {
        next()
        return
      }
      next({ name: 'SessionBootRoute', replace: true })
      return
    case 'denied':
      next({
        name: 'AccessDeniedRoute',
        replace: true,
        query: outcome.from ? { from: outcome.from } : undefined,
      })
      return
    default:
      next({ name: 'AccessDeniedRoute', replace: true })
  }
}
