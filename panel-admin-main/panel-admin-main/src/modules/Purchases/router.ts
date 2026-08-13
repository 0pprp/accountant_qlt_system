import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const PurchasesRoutes: RouteRecordRaw = {
  path: '/purchases',
  name: 'PurchasesListRoute',
  component: () => import('./views/PurchasesView.vue'),
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'Purchase' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(PurchasesRoutes)
}
