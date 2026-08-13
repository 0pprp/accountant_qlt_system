import { createRouter, createWebHistory } from 'vue-router'
import { authGuard } from './guard'

export const router = createRouter({
  history: createWebHistory(),
  routes: [],
  scrollBehavior() {
    return { top: 0 }
  },
})

router.beforeEach(authGuard)

export default router

export function isRouteRegistered(routeName: string): boolean {
  return router.hasRoute(routeName)
}

export function getRegisteredRoutes() {
  return router.getRoutes()
}
