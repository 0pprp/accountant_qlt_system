import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const ExpensesRoutes: RouteRecordRaw = {
  path: '/expenses',
  name: 'ExpensesListRoute',
  component: () => import('./views/ExpensesView.vue'),
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'Expense' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(ExpensesRoutes)
}
