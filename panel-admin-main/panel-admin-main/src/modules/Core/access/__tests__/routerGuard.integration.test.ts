import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import {
  createMemoryHistory,
  createRouter,
  type RouteRecordRaw,
} from 'vue-router'
import { defineComponent, h } from 'vue'

vi.mock('@/modules/Auth/session/bootstrapSession', () => ({
  bootstrapSession: vi.fn(() => Promise.resolve(true)),
  retrySessionBootstrap: vi.fn(() => Promise.resolve(true)),
  resetBootstrapStateForTests: vi.fn(),
}))

vi.mock('@/modules/Core/plugins/vue-query', () => ({
  clearSessionQueryCache: vi.fn(),
  cancelBranchQueries: vi.fn(),
  invalidateSessionQueries: vi.fn(),
  getQueryClient: vi.fn(),
  vueQuery: vi.fn(),
}))

import useAuthStore from '@/modules/Auth/store'
import { authGuard, resetGuardBootstrapFlagForTests } from '@/router/guard'
import type { AuthLoginSuccessResponse } from '@/modules/Auth/types/api'

function authPayload(
  partial: Partial<AuthLoginSuccessResponse> & {
    accessToken: string
    refreshToken: string
  }
): AuthLoginSuccessResponse {
  return {
    permissions: { OrderList: [{ name: 'Read', id: 1 }] },
    branches: [{ id: 1, name: 'فرع' } as never],
    ...partial,
  }
}

const stub = () => defineComponent({ setup: () => () => h('div') })

describe('تكامل Router — التحميل والمسارات غير المعلنة', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    resetGuardBootstrapFlagForTests()
  })

  it('ط) أثناء loading لا يُستدعى beforeEnter للصفحة المحمية', async () => {
    const enterSpy = vi.fn()

    const routes: RouteRecordRaw[] = [
      { path: '/session-boot', name: 'SessionBootRoute', component: stub() },
      {
        path: '/payment',
        name: 'PaymentListRoute',
        component: stub(),
        beforeEnter: () => {
          enterSpy()
        },
        meta: {
          requiresAuth: true,
          permission: { module: 'OrderList', action: 'Read' },
        },
      },
      { path: '/auth', name: 'AuthLoginRoute', component: stub() },
      { path: '/access-denied', name: 'AccessDeniedRoute', component: stub() },
      { path: '/', name: 'HomeMainRoute', component: stub() },
    ]

    const router = createRouter({
      history: createMemoryHistory(),
      routes,
    })
    router.beforeEach(authGuard)

    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.convertedPermissions = null
    auth.permissions = null
    auth.permissionsStatus = 'loading'

    await router.push('/payment')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('SessionBootRoute')
    expect(enterSpy).not.toHaveBeenCalled()
  })

  it('ح) مسار غير معلن بعد الجاهزية لا يُركّب صفحة أعمال', async () => {
    const enterSpy = vi.fn()

    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        {
          path: '/ghost',
          name: 'GhostBusinessRoute',
          component: stub(),
          beforeEnter: () => {
            enterSpy()
          },
        },
        { path: '/access-denied', name: 'AccessDeniedRoute', component: stub() },
        { path: '/auth', name: 'AuthLoginRoute', component: stub() },
        { path: '/session-boot', name: 'SessionBootRoute', component: stub() },
        { path: '/', name: 'HomeMainRoute', component: stub() },
      ],
    })
    router.beforeEach(authGuard)

    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))

    await router.push('/ghost')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('AccessDeniedRoute')
    expect(enterSpy).not.toHaveBeenCalled()
  })

  it('بعد الجاهزية بصلاحية تُفتح الوجهة ويُستدعى beforeEnter مرة واحدة', async () => {
    const enterSpy = vi.fn()
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        {
          path: '/payment',
          name: 'PaymentListRoute',
          component: stub(),
          beforeEnter: () => {
            enterSpy()
          },
          meta: {
            requiresAuth: true,
            permission: { module: 'OrderList', action: 'Read' },
          },
        },
        { path: '/session-boot', name: 'SessionBootRoute', component: stub() },
        { path: '/auth', name: 'AuthLoginRoute', component: stub() },
        { path: '/access-denied', name: 'AccessDeniedRoute', component: stub() },
        { path: '/', name: 'HomeMainRoute', component: stub() },
      ],
    })
    router.beforeEach(authGuard)

    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))

    await router.push('/payment')
    await router.isReady()

    expect(router.currentRoute.value.name).toBe('PaymentListRoute')
    expect(enterSpy).toHaveBeenCalledTimes(1)
  })
})
