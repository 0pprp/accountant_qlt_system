import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesCompletedRoutes: RouteRecordRaw = {
  path: '/sale/completed',
  name: 'SaleCompletedRoute',
  component: () => import('./views/SalesCompletedPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'Order', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesCompletedRoutes)
}
