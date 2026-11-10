import { watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/modules/Auth/store'
import { resolveRouteAccess } from '@/modules/Core/access/resolveRouteAccess'

/**
 * عند تغيّر الصلاحيات أو المستخدم داخل الجلسة — أعد تقييم المسار الحالي.
 * لا يُبقي المستخدم في قسم فقد صلاحيته.
 */
export function useRouteAccessRevalidation() {
  const authStore = useAuthStore()
  const route = useRoute()
  const router = useRouter()

  watch(
    () =>
      [
        authStore.permissionsStatus,
        authStore.convertedPermissions,
        authStore.sessionGeneration,
        route.fullPath,
      ] as const,
    () => {
      if (authStore.permissionsStatus !== 'ready') return
      if (!authStore.isAuthenticated) return

      const outcome = resolveRouteAccess({
        to: route,
        isAuthenticated: authStore.isAuthenticated,
        permissionsStatus: authStore.permissionsStatus,
        hasPermission: authStore.hasPermission,
      })

      if (outcome.type === 'denied' && route.name !== 'AccessDeniedRoute') {
        void router.replace({
          name: 'AccessDeniedRoute',
          query: outcome.from ? { from: outcome.from } : undefined,
        })
      }
    }
  )
}
