import type { Router, RouteRecordRaw } from 'vue-router'
import type {
  PermissionAction,
  PermissionModule,
} from '@/modules/Auth/types/model'

export const ActivityLogRoutes: RouteRecordRaw = {
  path: '/activity-log',
  name: 'ActivityLogMainRoute',
  children: [
    {
      path: '',
      name: 'ActivityLogListRoute',
      component: () => import('./views/ActivityLogListPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'ActivityLog' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
    {
      path: ':id',
      name: 'ActivityLogDetailRoute',
      component: () => import('./views/ActivityLogDetailPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'ActivityLog' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
  ],
  meta: {
    layout: 'Default',
  },
}

export default (router: Router) => {
  router.addRoute(ActivityLogRoutes)
}
