import type { Router, RouteRecordRaw } from 'vue-router'
import { SALES_MANAGER_SECTIONS } from './domain/sections'

const board = () => import('./views/SalesManagerBoard.vue')

export default function registerSalesManagerRouter(router: Router) {
  const routes = router.getRoutes()
  for (const section of SALES_MANAGER_SECTIONS) {
    const existing = routes.find((r) => r.name === section.routeName)
    if (!existing) continue

    const parent = routes.find((r) => r.children?.some((c) => c.name === section.routeName))
    const childRaw = parent?.children?.find((c) => c.name === section.routeName)
    const meta = { layout: 'Default', ...(parent?.meta ?? {}), ...existing.meta, salesManagerSection: section.key }

    if (parent?.name && childRaw) {
      router.addRoute(parent.name, { path: childRaw.path, name: section.routeName, component: board, meta } as RouteRecordRaw)
    } else {
      router.addRoute({ path: existing.path, name: section.routeName, component: board, meta } as RouteRecordRaw)
    }
  }
}
