import type { Router, RouteRecordRaw } from 'vue-router'
import { panelTabRequirement } from '@/modules/Core/navigation/panelTabAccess'

export const ArchiveRoutes: RouteRecordRaw = {
  path: '/archive',
  redirect: { name: 'ArchiveMainRoute' },
  children: [
    {
      path: '',
      name: 'ArchiveMainRoute',
      component: () => import('./views/ArchiveWorkspace.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        accessRequirement: panelTabRequirement('archive'),
      },
    },
  ],
  meta: { layout: 'Default' },
}

export default (router: Router) => {
  router.addRoute(ArchiveRoutes)
}
