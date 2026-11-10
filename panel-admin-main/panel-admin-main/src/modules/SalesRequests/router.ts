import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'
import { TAB_PERMISSION_ACTIONS } from './domain/tabs'

const tabAccessRequirement = {
  type: 'anyOf' as const,
  permissions: TAB_PERMISSION_ACTIONS.map((action) => ({
    module: 'SalesRequest' as PermissionModule,
    action: action as PermissionAction,
  })) as [
    { module: PermissionModule; action: PermissionAction },
    ...{ module: PermissionModule; action: PermissionAction }[],
  ],
}

export const SalesRequestsRoutes: RouteRecordRaw = {
  path: '/sale/requests',
  name: 'SalesRequestsMainRoute',
  redirect: { name: 'SalesRequestsWorkspaceRoute' },
  children: [
    {
      path: '',
      name: 'SalesRequestsWorkspaceRoute',
      component: () => import('./views/SalesRequestsWorkspace.vue'),
      meta: {
        requiresAuth: true,
        accessRequirement: tabAccessRequirement,
      },
    },
    {
      path: 'unassigned',
      name: 'UnassignedSalesRequestsRoute',
      component: () => import('./views/UnassignedSalesRequestsPage.vue'),
      meta: {
        requiresAuth: true,
        accessRequirement: tabAccessRequirement,
      },
    },
  ],
  meta: {
    layout: 'Default',
  },
}

export default (router: Router) => {
  router.addRoute(SalesRequestsRoutes)
}
