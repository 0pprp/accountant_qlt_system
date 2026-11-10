import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesRoutesRoutes: RouteRecordRaw = {
  path: '/sale/routes',
  name: 'SaleRoutesRoute',
  component: () => import('./views/SalesRoutesPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'SalesRoute', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesRoutesRoutes)
}
