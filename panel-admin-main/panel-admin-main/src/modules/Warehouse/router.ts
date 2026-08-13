import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const WarehouseRoutes: RouteRecordRaw = {
  path: '/warehouse',
  name: 'WarehouseListRoute',
  component: () => import('./views/WarehouseListPage.vue'),
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'ProductCategory' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(WarehouseRoutes)
}
