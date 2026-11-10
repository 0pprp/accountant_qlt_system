import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesSummaryRoutes: RouteRecordRaw = {
  path: '/sale/summary',
  name: 'SaleSummaryRoute',
  component: () => import('./views/SalesSummaryPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'SalesSummary', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesSummaryRoutes)
}
