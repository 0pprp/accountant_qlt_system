import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const CustomerRoutes: RouteRecordRaw = {
  path: '/customer',
  redirect: { name: 'CustomerMainRoute' },
  children: [
    {
      path: '',
      name: 'CustomerMainRoute',
      component: () => import('./views/CustomerView.vue'),
      meta: {
        layout: 'Default',
        requiresAuth: true,
        permission: {
          module: 'Customer' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
    {
      path: 'create',
      name: 'CreateCustomerView',
      component: () => import('./views/CustomerUpsertView.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'Customer' as PermissionModule,
          action: 'Create' as PermissionAction,
        },
      },
    },
    {
      path: ':id/edit',
      name: 'EditCustomerView',
      component: () => import('./views/CustomerUpsertView.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'Customer' as PermissionModule,
          action: 'Update' as PermissionAction,
        },
      },
    },
  ],
  meta: {
    layout: 'Default',
  },
}

export default (router: Router) => {
  router.addRoute(CustomerRoutes)
}
