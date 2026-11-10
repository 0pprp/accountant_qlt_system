import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesFollowupRoutes: RouteRecordRaw = {
  path: '/sale/followup',
  name: 'SalesFollowupRoute',
  component: () => import('./views/SalesFollowupPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'SalesFollowup', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesFollowupRoutes)
}
