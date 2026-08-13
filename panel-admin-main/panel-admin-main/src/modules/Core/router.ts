import type { Router, RouteRecordRaw } from 'vue-router'
import HomeView from './views/HomeView.vue'

export const MainRoutes: RouteRecordRaw = {
  path: '/',
  name: 'HomeMainRoute',
  component: HomeView,
  meta: {
    layout: 'Default',
  },
}

export default (router: Router) => {
  router.addRoute(MainRoutes)
}
