import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesExcelSearchRoutes: RouteRecordRaw = {
  path: '/sale/excel-search',
  name: 'SaleExcelSearchRoute',
  component: () => import('./views/SalesExcelSearchPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'Customer', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesExcelSearchRoutes)
}
