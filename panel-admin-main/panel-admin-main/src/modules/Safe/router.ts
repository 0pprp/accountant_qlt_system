import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const SafeRoutes: RouteRecordRaw = {
  path: '/safe',
  redirect: { name: 'SafeMainRoute' },
  children: [
    {
      path: '',
      name: 'SafeMainRoute',
      component: () => import('./views/SafeView.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        permission: {
          module: 'Safe' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
    {
      path: 'main',
      name: 'SafeMainMainRoute',
      component: () => import('./views/SafeView.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        permission: {
          module: 'Safe' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
    {
      path: 'transactions',
      name: 'SafeTransactionsRoute',
      component: () => import('./views/SafeTransactionView.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        permission: {
          module: 'Transaction' as PermissionModule,
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
  router.addRoute(SafeRoutes)
}
