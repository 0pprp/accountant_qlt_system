import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const PaymentRoutes: RouteRecordRaw = {
  path: '/payment',
  name: 'PaymentMainRoute',
  children: [
    {
      path: '',
      name: 'PaymentListRoute',
      component: () => import('./views/PaymentListPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'OrderList' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
    {
      path: '/payment/:orderListId/installments',
      name: 'InstallmentPaymentListRoute',
      component: () => import('./views/InstallmentPaymentListPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'InstallmentPayment' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },
  ],
  meta: {
    layout: 'Default',
  },
}

export default (router: Router) => {
  router.addRoute(PaymentRoutes)
}
