import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const BranchRoutes: RouteRecordRaw = {
  path: '/branch',
  name: 'BranchListRoute',
  component: () => import('./views/BranchListPage.vue'),
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'Branch' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(BranchRoutes)
}
