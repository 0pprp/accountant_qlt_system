import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import {
  AxiosError,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios'

const postRefreshToken = vi.fn()
const toastSet = vi.fn()
const routerPush = vi.fn(() => Promise.resolve())

vi.mock('@/modules/Auth/session/refreshHttp', () => ({
  postRefreshToken: (...args: unknown[]) => postRefreshToken(...args),
}))

vi.mock('@/router', () => ({
  default: {
    push: (...args: unknown[]) => routerPush(...args),
    replace: vi.fn(() => Promise.resolve()),
  },
}))

vi.mock('@/modules/Core/plugins/vue-query', () => ({
  clearSessionQueryCache: vi.fn(),
  cancelBranchQueries: vi.fn(),
  invalidateSessionQueries: vi.fn(),
  getQueryClient: vi.fn(),
  vueQuery: vi.fn(),
}))

vi.mock('@/modules/Core/store', () => ({
  useToastStore: () => ({
    setMassage: toastSet,
    clearToast: vi.fn(),
  }),
}))

vi.mock('@/modules/Core/utils', () => ({
  getLocalizationTitle: (key: string) => key,
}))

import useAuthStore from '@/modules/Auth/store'
import {
  axiosInstance,
  getAxiosQueueSizeForTests,
  getRefreshOwnerGenerationForTests,
  resetAxiosRefreshStateForTests,
} from '@/modules/Core/plugins/axios'
import { resetSessionRefreshCoordinatorForTests } from '@/modules/Auth/session/sessionRefreshCoordinator'
import { resetBootstrapStateForTests } from '@/modules/Auth/session/bootstrapSession'
import { isAbortError } from '@/modules/Auth/session/sessionAbort'
import {
  invalidateSessionQueries,
  cancelBranchQueries,
} from '@/modules/Core/plugins/vue-query'
import type { AuthLoginSuccessResponse } from '@/modules/Auth/types/api'

type Pending = {
  config: InternalAxiosRequestConfig
  resolve: (value: AxiosResponse) => void
  reject: (reason?: unknown) => void
}

function deferred<T>() {
  let resolve!: (value: T) => void
  let reject!: (reason?: unknown) => void
  const promise = new Promise<T>((res, rej) => {
    resolve = res
    reject = rej
  })
  return { promise, resolve, reject }
}

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

async function flush() {
  await Promise.resolve()
  await Promise.resolve()
}

describe('تكامل Axios + Pinia — عزل الجلسة', () => {
  let pending: Pending[]

  beforeEach(() => {
    setActivePinia(createPinia())
    resetAxiosRefreshStateForTests()
    resetSessionRefreshCoordinatorForTests()
    resetBootstrapStateForTests()
    toastSet.mockClear()
    routerPush.mockClear()
    postRefreshToken.mockReset()
    vi.mocked(cancelBranchQueries).mockClear()
    vi.mocked(invalidateSessionQueries).mockClear()
    pending = []

    axiosInstance.defaults.adapter = (config) =>
      new Promise<AxiosResponse>((resolve, reject) => {
        pending.push({
          config: config as InternalAxiosRequestConfig,
          resolve,
          reject,
        })
      })
  })

  async function waitPending(min = 1) {
    await vi.waitFor(() => {
      expect(pending.length).toBeGreaterThanOrEqual(min)
    })
  }

  function settleOk(index: number, data: unknown = { ok: true }) {
    const item = pending[index]
    item.resolve({
      data,
      status: 200,
      statusText: 'OK',
      headers: {},
      config: item.config,
    })
  }

  function settleStatus(index: number, status: number, data: unknown = {}) {
    const item = pending[index]
    item.reject(
      new AxiosError(
        `Request failed with status ${status}`,
        String(status),
        item.config,
        null,
        {
          status,
          data,
          headers: {},
          config: item.config,
          statusText: String(status),
        }
      )
    )
  }

  it('أ) طلب A يرجع 200 بعد دخول B — لا تُمرَّر النتيجة لجلسة B', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    const genA = auth.sessionGeneration

    const requestA = axiosInstance.get('/admin/data')
    await waitPending(1)
    expect(
      (pending[0].config as { sessionGeneration?: number }).sessionGeneration
    ).toBe(genA)

    auth.replaceSession(authPayload({ accessToken: 'B', refreshToken: 'RB' }))
    settleOk(0, { secret: 'from-A' })

    await expect(requestA).rejects.toSatisfy(isAbortError)
    expect(auth.accessToken).toBe('B')
    expect(toastSet).not.toHaveBeenCalled()
  })

  it('ب) طلب A يرجع 401 بعد دخول B — لا refresh لـ B ولا إعادة بتوكن B', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))

    const requestA = axiosInstance.get('/admin/secure')
    await waitPending(1)

    auth.replaceSession(authPayload({ accessToken: 'B', refreshToken: 'RB' }))
    settleStatus(0, 401)

    await expect(requestA).rejects.toSatisfy(isAbortError)
    expect(postRefreshToken).not.toHaveBeenCalled()
    expect(getRefreshOwnerGenerationForTests()).toBeNull()
    expect(auth.accessToken).toBe('B')
    expect(auth.isAuthenticated).toBe(true)
  })

  it('ج) فشل/اكتمال refresh لجيل A بعد دخول B لا يُنهي جلسة B', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    const genA = auth.sessionGeneration

    auth.replaceSession(authPayload({ accessToken: 'B', refreshToken: 'RB' }))

    const applied = auth.applyAuthPayload(
      authPayload({ accessToken: 'A-late', refreshToken: 'RA-late' }),
      genA,
      { source: 'refresh' }
    )
    expect(applied).toBe(false)
    expect(auth.accessToken).toBe('B')
    expect(auth.isAuthenticated).toBe(true)
  })

  it('د) طلبان 401 متزامنان — refresh واحد والطلبان يكملان', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))

    const refreshGate = deferred<AuthLoginSuccessResponse>()
    postRefreshToken.mockImplementation(() => refreshGate.promise)

    const r1 = axiosInstance.get('/admin/one')
    const r2 = axiosInstance.get('/admin/two')
    await waitPending(2)

    settleStatus(0, 401)
    settleStatus(1, 401)

    await vi.waitFor(() => {
      expect(postRefreshToken).toHaveBeenCalledTimes(1)
    })

    expect(getRefreshOwnerGenerationForTests()).toBe(auth.sessionGeneration)

    const beforeRetries = pending.length
    refreshGate.resolve(
      authPayload({ accessToken: 'A2', refreshToken: 'RA2' })
    )

    await vi.waitFor(() => {
      expect(pending.length).toBeGreaterThanOrEqual(beforeRetries + 2)
    })

    settleOk(pending.length - 2, { ok: true })
    settleOk(pending.length - 1, { ok: true })

    await expect(r1).resolves.toMatchObject({ status: 200 })
    await expect(r2).resolves.toMatchObject({ status: 200 })
    expect(auth.accessToken).toBe('A2')
    expect(postRefreshToken).toHaveBeenCalledTimes(1)
  })

  it('هـ) bootstrap و401 يشتركان في refresh واحد لنفس الجيل', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.markPermissionsLoading()

    const refreshGate = deferred<AuthLoginSuccessResponse>()
    postRefreshToken.mockImplementation(() => refreshGate.promise)

    const { bootstrapSession } = await import(
      '@/modules/Auth/session/bootstrapSession'
    )
    const boot = bootstrapSession()
    await flush()
    expect(postRefreshToken).toHaveBeenCalledTimes(1)

    const r1 = axiosInstance.get('/admin/while-boot')
    await waitPending(1)
    settleStatus(0, 401)

    await flush()
    expect(postRefreshToken).toHaveBeenCalledTimes(1)

    refreshGate.resolve(
      authPayload({ accessToken: 'A2', refreshToken: 'RA2' })
    )

    await expect(boot).resolves.toBe(true)
    expect(postRefreshToken).toHaveBeenCalledTimes(1)

    await vi.waitFor(() => expect(pending.length).toBeGreaterThan(1))
    settleOk(pending.length - 1)
    await expect(r1).resolves.toMatchObject({ status: 200 })
  })

  it('و) انتهاء الجلسة يرفض الطابور ولا يعلّق الوعود', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))

    const refreshGate = deferred<AuthLoginSuccessResponse>()
    postRefreshToken.mockImplementation(() => refreshGate.promise)

    const waiting = axiosInstance.get('/admin/queued')
    await waitPending(1)
    settleStatus(0, 401)

    await vi.waitFor(() => expect(postRefreshToken).toHaveBeenCalled())
    await vi.waitFor(() =>
      expect(getRefreshOwnerGenerationForTests()).toBe(auth.sessionGeneration)
    )

    // طلب ثانٍ أثناء refresh → يُضاف للطابور مباشرة عبر 401
    const waiting2 = axiosInstance.get('/admin/queued-2')
    await waitPending(2)
    expect(pending[1].config.headers?.Authorization).toBeTruthy()
    settleStatus(1, 401)

    await vi.waitFor(() =>
      expect(getAxiosQueueSizeForTests()).toBeGreaterThan(0)
    )

    auth.endSession('logout')

    await expect(waiting).rejects.toSatisfy(isAbortError)
    await expect(waiting2).rejects.toSatisfy(isAbortError)
    expect(getAxiosQueueSizeForTests()).toBe(0)
  })

  it('ز) refresh بنفس الصلاحيات لا يستدعي cancelBranchQueries', () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    vi.mocked(cancelBranchQueries).mockClear()
    vi.mocked(invalidateSessionQueries).mockClear()

    const applied = auth.applyAuthPayload(
      authPayload({ accessToken: 'A2', refreshToken: 'RA2' }),
      auth.sessionGeneration,
      { source: 'refresh' }
    )
    expect(applied).toBe(true)
    expect(cancelBranchQueries).not.toHaveBeenCalled()
    expect(invalidateSessionQueries).not.toHaveBeenCalled()
  })
})
