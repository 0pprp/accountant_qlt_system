import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const RoleRoutes: RouteRecordRaw = {
  path: '/role',
  name: 'RoleMainRoute',
  redirect: { name: 'RoleMainRoute' },
  children: [],
  meta: {
    layout: 'Default',
    requiresAuth: true,
    permission: {
      module: 'Role' as PermissionModule,
      action: 'Read' as PermissionAction,
    },
  },
}

export default (router: Router) => {
  router.addRoute(RoleRoutes)
}
