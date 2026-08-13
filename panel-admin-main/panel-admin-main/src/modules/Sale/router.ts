import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const SaleRoutes: RouteRecordRaw = {
  path: '/sale',
  name: 'SaleMainRoute',
  children: [
    {
      path: '',
      name: 'SaleListRoute',
      component: () => import('./views/SaleListPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'Order' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
    {
      path: '/sale/create',
      name: 'SaleCreateRoute',
      component: () => import('./views/SaleCreatePage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'Order' as PermissionModule,
          action: 'Create' as PermissionAction,
        },
      },
    },

    {
      path: '/sale/:saleId/update',
      name: 'SaleUpdateRoute',
      component: () => import('./views/SaleManagementPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'Order' as PermissionModule,
          action: 'Update' as PermissionAction,
        },
      },
    },

    {
      path: '/sale/:saleId/view',
      name: 'SaleViewRoute',
      component: () => import('./views/SaleManagementPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'Order' as PermissionModule,
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
  router.addRoute(SaleRoutes)
}
