import type { Router, RouteRecordRaw } from 'vue-router'
import { panelTabRequirement } from '@/modules/Core/navigation/panelTabAccess'

export const SafeRoutes: RouteRecordRaw = {
  path: '/safe',
  redirect: { name: 'SafeMainRoute' },
  children: [
    {
      path: '',
      name: 'SafeMainRoute',
      component: () => import('./views/SafeWorkspace.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        accessRequirement: panelTabRequirement('safe'),
      },
    },
    {
      path: ':id/movements',
      name: 'SafeMovementsRoute',
      component: () => import('./views/SafeMovementsPage.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        accessRequirement: panelTabRequirement('safe'),
      },
    },
  ],
  meta: { layout: 'Default' },
}

export default (router: Router) => {
  router.addRoute(SafeRoutes)
}
