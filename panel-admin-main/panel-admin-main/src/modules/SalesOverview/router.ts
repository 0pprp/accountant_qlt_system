import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesOverviewRoutes: RouteRecordRaw = {
  path: '/sale/overview',
  name: 'SaleOverviewRoute',
  component: () => import('./views/SalesOverviewPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'SalesOverview', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesOverviewRoutes)
}
