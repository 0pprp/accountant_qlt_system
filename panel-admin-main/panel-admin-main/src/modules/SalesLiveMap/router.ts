import type { Router, RouteRecordRaw } from 'vue-router'

export const SalesLiveMapRoutes: RouteRecordRaw = {
  path: '/sale/live-map',
  name: 'SaleLiveMapRoute',
  component: () => import('./views/SalesLiveMapPage.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
    accessRequirement: {
      type: 'single',
      permission: { module: 'SalesLocation', action: 'Read' },
    },
  },
}

export default (router: Router) => {
  router.addRoute(SalesLiveMapRoutes)
}
