import type { Router, RouteRecordRaw } from 'vue-router'

export const AuthRoutes: RouteRecordRaw = {
  path: '/auth',
  name: 'AuthLoginRoute',
  component: () => import('./views/LoginPage.vue'),
  children: [],
  meta: {
    layout: 'Auth',
  },
}

export default (router: Router) => {
  router.addRoute(AuthRoutes)
}
