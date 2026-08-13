import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const OrderRoutes: RouteRecordRaw = {
  path: '/order',
  name: 'OrderListRoute',
  component: () => import('./views/OrderListPage.vue'),
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'OrderList' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(OrderRoutes)
}
