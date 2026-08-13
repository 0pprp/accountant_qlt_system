import type { NavigationGuardNext, RouteLocationNormalized } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store/index'

export function authGuard(
  to: RouteLocationNormalized,
  from: RouteLocationNormalized,
  next: NavigationGuardNext
) {
  const authStore = useAuthStore()

  const publicRoutes = ['AuthLoginRoute']
  const isPublicRoute = publicRoutes.includes(to.name as string)

  if (!authStore.isAuthenticated && !isPublicRoute) {
    next({ name: 'AuthLoginRoute' })
    return
  }

  if (authStore.isAuthenticated && to.name === 'AuthLoginRoute') {
    next({ name: 'HomeMainRoute' })
    return
  }

  if (authStore.isAuthenticated && to.meta.requiresAuth && to.meta.permission) {
    const { module, action } = to.meta.permission

    const hasAccess = authStore.hasPermission(module, action)

    if (!hasAccess) {
      next({ name: 'HomeMainRoute', replace: true })
      return
    }
  }

  next()
}
