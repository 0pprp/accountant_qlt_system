import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesEmployeesRoutes: RouteRecordRaw = {
  path: '/sale/employees',
  name: 'SaleEmployeesRoute',
  component: () => import('./views/SalesEmployeesPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'SalesEmployee', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesEmployeesRoutes)
}
