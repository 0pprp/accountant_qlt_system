import type { Router, RouteRecordRaw } from 'vue-router'
import type { PermissionAction, PermissionModule } from '../Auth/types/model'

export const UserRoutes: RouteRecordRaw = {
  path: '/user',
  name: 'UserMainRoute',
  children: [
    {
      path: '',
      name: 'UserListRoute',
      component: () => import('./views/UserListPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'User' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },

    {
      path: '/user/create',
      name: 'UserCreateRoute',
      component: () => import('./views/UserCreatePage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'User' as PermissionModule,
          action: 'Create' as PermissionAction,
        },
      },
    },

    {
      path: '/user/:userId/update',
      name: 'UserUpdateRoute',
      component: () => import('./views/UserManagementPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'User' as PermissionModule,
          action: 'Update' as PermissionAction,
        },
      },
    },

    {
      path: '/user/:userId/view',
      name: 'UserViewRoute',
      component: () => import('./views/UserManagementPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'User' as PermissionModule,
          action: 'Read' as PermissionAction,
        },
      },
    },

    {
      path: '/user/:userId/daily-report',
      name: 'UserDailyReportRoute',
      component: () => import('./views/UserDailyReportPage.vue'),
      meta: {
        requiresAuth: true,
        permission: {
          module: 'User' as PermissionModule,
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
  router.addRoute(UserRoutes)
}
