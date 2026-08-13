import type { Router, RouteRecordRaw } from 'vue-router'
import type {
  PermissionAction,
  PermissionModule,
} from '@/modules/Auth/types/model'

export const NotificationRoutes: RouteRecordRaw = {
  path: '/notifications',
  name: 'NotificationListRoute',
  component: () => import('./views/NotificationListPage.vue'),
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'Notification' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(NotificationRoutes)
}
