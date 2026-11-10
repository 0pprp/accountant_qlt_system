import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesLegalRoutes: RouteRecordRaw = {
  path: '/sale/legal',
  name: 'SaleLegalFollowupRoute',
  component: () => import('./views/SalesLegalPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'LegalFollowup', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesLegalRoutes)
}
