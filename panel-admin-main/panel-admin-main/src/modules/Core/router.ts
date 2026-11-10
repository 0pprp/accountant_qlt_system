import type { Router, RouteRecordRaw } from 'vue-router'
import HomeView from './views/HomeView.vue'

export const MainRoutes: RouteRecordRaw = {
  path: '/',
  name: 'HomeMainRoute',
  component: HomeView,
  meta: {
    layout: 'Default',
    requiresAuth: true,
  },
}

export const AccessDeniedRoute: RouteRecordRaw = {
  path: '/access-denied',
  name: 'AccessDeniedRoute',
  component: () => import('./views/AccessDeniedView.vue'),
  meta: {
    layout: 'Default',
    requiresAuth: true,
  },
}

/** انتظار تهيئة الجلسة — بلا بيانات أعمال */
export const SessionBootRoute: RouteRecordRaw = {
  path: '/session-boot',
  name: 'SessionBootRoute',
  component: () => import('./views/SessionBootView.vue'),
  meta: {
    layout: 'Auth',
    publicSession: true,
  },
}

/** 404 معلن صراحةً — ليس سماحًا ضمنيًا لمسار أعمال */
export const NotFoundRoute: RouteRecordRaw = {
  path: '/:pathMatch(.*)*',
  name: 'NotFoundRoute',
  component: () => import('./views/NotFoundView.vue'),
  meta: {
    layout: 'Auth',
    publicSession: true,
  },
}

/** معاينة مكوّنات العرض — تطوير فقط؛ لا صلاحيات أعمال ولا endpoints. */
export const UiKitPreviewRoute: RouteRecordRaw = {
  path: '/dev/ui-kit',
  name: 'UiKitPreviewRoute',
  component: () => import('./views/UiKitPreviewView.vue'),
  meta: {
    // بلا Auth/Default layout — غلاف div فقط حتى لا يُقص المحتوى (overflow Auth).
    // DEV-only (beforeEnter + registration). No business permission; no endpoints.
    publicSession: true,
    uiKitPreview: true,
  },
  beforeEnter: (_to, _from, next) => {
    if (import.meta.env.DEV) next()
    else next({ name: 'NotFoundRoute' })
  },
}

export default (router: Router) => {
  router.addRoute(MainRoutes)
  router.addRoute(AccessDeniedRoute)
  router.addRoute(SessionBootRoute)
  if (import.meta.env.DEV) {
    router.addRoute(UiKitPreviewRoute)
  }
  router.addRoute(NotFoundRoute)
}
