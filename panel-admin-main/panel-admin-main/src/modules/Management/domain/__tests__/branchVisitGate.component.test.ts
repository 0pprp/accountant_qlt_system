import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  createApp,
  defineComponent,
  h,
  nextTick,
  onMounted,
  type App,
} from 'vue'
import { createPinia, getActivePinia, setActivePinia } from 'pinia'
import {
  createMemoryHistory,
  createRouter,
  RouterView,
  type Router,
  type RouteRecordRaw,
} from 'vue-router'
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'
import { createI18n } from 'vue-i18n'
import type { InternalAxiosRequestConfig } from 'axios'

vi.mock('@/modules/Core/plugins/vue-query', async (importOriginal) => {
  const actual =
    await importOriginal<typeof import('@/modules/Core/plugins/vue-query')>()
  return {
    ...actual,
    clearSessionQueryCache: vi.fn(),
    cancelBranchQueries: vi.fn(),
    invalidateSessionQueries: vi.fn(),
  }
})

import ManagementWorkspace from '../../views/ManagementWorkspace.vue'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import useAuthStore from '@/modules/Auth/store'
import type { AuthLoginSuccessResponse } from '@/modules/Auth/types/api'

type Tracked = { method: string; url: string }

function authPayload(
  partial: Partial<AuthLoginSuccessResponse> & {
    accessToken: string
    refreshToken: string
  }
): AuthLoginSuccessResponse {
  return {
    permissions: {
      Customer: [{ name: 'Read', id: 1 }],
      BranchVisit: [
        { name: 'Read', id: 2 },
        { name: 'Open', id: 3 },
        { name: 'Depart', id: 4 },
      ],
      BranchNewSale: [{ name: 'Read', id: 5 }],
    },
    branches: [
      { id: 1, name: 'فرع أ' } as never,
      { id: 2, name: 'فرع ب' } as never,
    ],
    ...partial,
  }
}

function isOpsUrl(url: string) {
  return (
    url.includes('/admin/BranchNewSales') ||
    url.includes('/admin/BranchCustomers') ||
    url.includes('/admin/BranchPayments') ||
    url.includes('/admin/BranchDeclining')
  )
}

function isActiveVisitUrl(url: string) {
  return url.includes('/admin/BranchVisits/active')
}

/** صفحة أعمال حقيقية من ناحية API: تطلق الطلب عند التركيب فقط. */
const OpsProbePage = defineComponent({
  name: 'OpsProbePage',
  setup() {
    onMounted(() => {
      void axiosInstance.get('/admin/BranchNewSales', {
        params: { page: 1, pageSize: 50 },
      })
    })
    return () => h('div', { 'data-testid': 'ops-probe' }, 'ops')
  },
})

const VisitProbePage = defineComponent({
  name: 'VisitProbePage',
  setup() {
    return () => h('div', { 'data-testid': 'visit-probe' }, 'visit')
  },
})

describe('بوابة زيارة الفرع — ManagementWorkspace الحقيقي + Router + API', () => {
  let app: App | null = null
  let host: HTMLElement | null = null
  let router: Router
  let tracked: Tracked[]
  let activeVisitResolver: ((value: unknown) => void) | null = null
  let activeVisitPayload: unknown = null
  let activeMode: 'delay' | 'immediate' | 'error' = 'immediate'

  function branchRoutes(): RouteRecordRaw[] {
    return [
      {
        path: '/management',
        name: 'ManagementMainRoute',
        component: ManagementWorkspace,
        children: [
          {
            path: 'branch/sales',
            name: 'ManagementBranchSalesRoute',
            component: OpsProbePage,
            meta: {
              managementSectionId: 'branch',
              managementTabId: 'sales',
            },
          },
          {
            path: 'branch/visit-workspace',
            name: 'ManagementBranchVisitWorkspaceRoute',
            component: VisitProbePage,
            meta: {
              managementSectionId: 'branch',
              managementTabId: 'branchVisitWorkspace',
            },
          },
        ],
      },
      {
        path: '/access-denied',
        name: 'AccessDeniedRoute',
        component: defineComponent({
          setup: () => () => h('div', 'denied'),
        }),
      },
    ]
  }

  async function mountAt(path: string) {
    const queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false, gcTime: 0 } },
    })
    const i18n = createI18n({
      legacy: false,
      locale: 'ar',
      missing: (_l, key) => key,
      messages: { ar: {} },
    })

    router = createRouter({
      history: createMemoryHistory(),
      routes: branchRoutes(),
    })

    host = document.createElement('div')
    document.body.appendChild(host)
    app = createApp({ setup: () => () => h(RouterView) })
    const pinia = getActivePinia()
    if (!pinia) throw new Error('pinia must be active before mountAt')
    app.use(pinia)
    app.use(router)
    app.use(i18n)
    app.use(VueQueryPlugin, { queryClient })

    await router.push(path)
    await router.isReady()
    app.mount(host)

    for (let i = 0; i < 25; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 15))
      if (host.querySelector('.mgmt-workspace')) break
    }
  }

  function opsCalls() {
    return tracked.filter((t) => isOpsUrl(t.url))
  }

  beforeEach(() => {
    tracked = []
    activeVisitResolver = null
    activeVisitPayload = null
    activeMode = 'immediate'

    axiosInstance.defaults.adapter = async (
      config: InternalAxiosRequestConfig
    ) => {
      const url = String(config.url ?? '')
      const method = String(config.method ?? 'get').toLowerCase()
      tracked.push({ method, url })

      if (isActiveVisitUrl(url)) {
        if (activeMode === 'delay') {
          const data = await new Promise((resolve, reject) => {
            activeVisitResolver = resolve
            void reject
          })
          return {
            data,
            status: 200,
            statusText: 'OK',
            headers: {},
            config,
          }
        }
        if (activeMode === 'error') {
          return Promise.reject(
            Object.assign(new Error('network'), {
              isAxiosError: true,
              config,
            })
          )
        }
        return {
          data: activeVisitPayload,
          status: 200,
          statusText: 'OK',
          headers: {},
          config,
        }
      }

      if (isOpsUrl(url)) {
        return {
          data: { items: [], totalCount: 0, unreadCount: 0, page: 1, pageSize: 50 },
          status: 200,
          statusText: 'OK',
          headers: {},
          config,
        }
      }

      return {
        data: null,
        status: 200,
        statusText: 'OK',
        headers: {},
        config,
      }
    }
  })

  afterEach(() => {
    app?.unmount()
    host?.remove()
    app = null
    host = null
    delete axiosInstance.defaults.adapter
  })

  it('رابط مباشر + زيارة مؤخرة → صفر طلبات أعمال؛ ثم بلا زيارة → visit-workspace بلا loop', async () => {
    activeMode = 'delay'
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/sales')

    expect(host!.querySelector('[data-testid="mgmt-ops-body"]')).toBeNull()
    expect(host!.querySelector('[data-testid="ops-probe"]')).toBeNull()
    expect(opsCalls()).toHaveLength(0)
    expect(tracked.some((t) => isActiveVisitUrl(t.url))).toBe(true)

    activeVisitResolver?.(null)
    for (let i = 0; i < 30; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 15))
      if (router.currentRoute.value.name === 'ManagementBranchVisitWorkspaceRoute')
        break
    }

    expect(router.currentRoute.value.name).toBe(
      'ManagementBranchVisitWorkspaceRoute'
    )
    expect(opsCalls()).toHaveLength(0)

    const before = router.currentRoute.value.fullPath
    await new Promise((r) => setTimeout(r, 80))
    expect(router.currentRoute.value.fullPath).toBe(before)
  })

  it('زيارة فرع آخر لا تركّب أعمال الفرع المحدد', async () => {
    activeMode = 'immediate'
    activeVisitPayload = { id: 9, branchId: 2, status: 'Open' }
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/sales')
    for (let i = 0; i < 25; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 15))
      if (router.currentRoute.value.name === 'ManagementBranchVisitWorkspaceRoute')
        break
    }

    expect(host!.querySelector('[data-testid="ops-probe"]')).toBeNull()
    expect(opsCalls()).toHaveLength(0)
    expect(router.currentRoute.value.name).toBe(
      'ManagementBranchVisitWorkspaceRoute'
    )
  })

  it('فشل الشبكة يمنع أعمال الفرع', async () => {
    activeMode = 'error'
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/sales')
    for (let i = 0; i < 25; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 15))
      if (router.currentRoute.value.name === 'ManagementBranchVisitWorkspaceRoute')
        break
    }

    expect(opsCalls()).toHaveLength(0)
    expect(host!.querySelector('[data-testid="ops-probe"]')).toBeNull()
  })

  it('نقص BranchVisit.Read لا يمنح تجاوزًا لأعمال الفرع', async () => {
    activeMode = 'immediate'
    activeVisitPayload = null
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(
      authPayload({
        accessToken: 'A',
        refreshToken: 'RA',
        permissions: {
          Customer: [{ name: 'Read', id: 1 }],
          BranchNewSale: [{ name: 'Read', id: 5 }],
        },
      })
    )
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/sales')
    for (let i = 0; i < 25; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 15))
      if (router.currentRoute.value.name === 'ManagementBranchVisitWorkspaceRoute')
        break
    }

    expect(opsCalls()).toHaveLength(0)
    expect(tracked.some((t) => isActiveVisitUrl(t.url))).toBe(false)
    expect(host!.querySelector('[data-testid="ops-probe"]')).toBeNull()
  })

  it('زيارة مطابقة تسمح بتركيب الأعمال وطلب القائمة', async () => {
    activeMode = 'immediate'
    activeVisitPayload = { id: 1, branchId: 1, status: 'Open' }
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/sales')
    for (let i = 0; i < 35; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 20))
      if (opsCalls().length > 0) break
    }

    expect(host!.querySelector('[data-testid="mgmt-ops-body"]')).not.toBeNull()
    expect(host!.querySelector('[data-testid="ops-probe"]')).not.toBeNull()
    expect(opsCalls().some((c) => c.url.includes('/admin/BranchNewSales'))).toBe(
      true
    )
  })

  it('تبديل الفرع / إبطال الزيارة يُخرج أعمال الفرع', async () => {
    activeMode = 'immediate'
    activeVisitPayload = { id: 1, branchId: 1, status: 'Open' }
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/sales')
    for (let i = 0; i < 35; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 20))
      if (host!.querySelector('[data-testid="ops-probe"]')) break
    }
    expect(host!.querySelector('[data-testid="ops-probe"]')).not.toBeNull()

    activeVisitPayload = null
    auth.changeSelectedBranch({ id: 2, name: 'فرع ب' } as never)
    for (let i = 0; i < 35; i++) {
      await nextTick()
      await new Promise((r) => setTimeout(r, 20))
      if (!host!.querySelector('[data-testid="ops-probe"]')) break
    }

    expect(host!.querySelector('[data-testid="ops-probe"]')).toBeNull()
  })

  it('دخول صفحة الزيارة مباشرة بلا حلقة توجيه', async () => {
    activeMode = 'immediate'
    activeVisitPayload = null
    setActivePinia(createPinia())
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.selectedBranch = { id: 1, name: 'فرع أ' } as never

    await mountAt('/management/branch/visit-workspace')
    await new Promise((r) => setTimeout(r, 100))

    expect(router.currentRoute.value.name).toBe(
      'ManagementBranchVisitWorkspaceRoute'
    )
    expect(opsCalls()).toHaveLength(0)
    expect(host!.querySelector('[data-testid="visit-probe"]')).not.toBeNull()
    expect(host!.querySelector('[data-testid="mgmt-ops-body"]')).not.toBeNull()
  })
})
