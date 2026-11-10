import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { AxiosError } from 'axios'

const postRefreshToken = vi.fn()

vi.mock('@/modules/Auth/session/refreshHttp', () => ({
  postRefreshToken: (...args: unknown[]) => postRefreshToken(...args),
}))

vi.mock('@/modules/Core/plugins/vue-query', () => ({
  clearSessionQueryCache: vi.fn(),
  cancelBranchQueries: vi.fn(),
  invalidateSessionQueries: vi.fn(),
  getQueryClient: vi.fn(),
  vueQuery: vi.fn(),
}))

vi.mock('@/router', () => ({
  default: {
    push: vi.fn(() => Promise.resolve()),
    replace: vi.fn(() => Promise.resolve()),
  },
}))

import useAuthStore from '@/modules/Auth/store'
import {
  bootstrapSession,
  resetBootstrapStateForTests,
  retrySessionBootstrap,
} from '@/modules/Auth/session/bootstrapSession'
import { resetSessionRefreshCoordinatorForTests } from '@/modules/Auth/session/sessionRefreshCoordinator'
import type { AuthLoginSuccessResponse } from '@/modules/Auth/types/api'

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

function networkError() {
  return new AxiosError('Network Error', 'ERR_NETWORK')
}

function invalidRefreshError() {
  return AxiosError.from(
    new Error('Unauthorized'),
    '401',
    undefined,
    null,
    {
      status: 401,
      data: {},
      statusText: 'Unauthorized',
      headers: {},
      config: {} as never,
    }
  )
}

describe('تكامل bootstrap — إعادة المحاولة بعد فشل مؤقت', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    resetSessionRefreshCoordinatorForTests()
    resetBootstrapStateForTests()
    postRefreshToken.mockReset()
  })

  it('فشل شبكة ثم retry يرسل طلبًا ثانيًا وينجح → permissions ready', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.markPermissionsLoading()

    postRefreshToken.mockRejectedValueOnce(networkError())

    await expect(bootstrapSession()).resolves.toBe(false)
    expect(postRefreshToken).toHaveBeenCalledTimes(1)
    expect(auth.permissionsStatus).toBe('error')
    expect(auth.refreshToken).toBe('RA')

    postRefreshToken.mockResolvedValueOnce(
      authPayload({ accessToken: 'A2', refreshToken: 'RA2' })
    )

    await expect(retrySessionBootstrap()).resolves.toBe(true)
    expect(postRefreshToken).toHaveBeenCalledTimes(2)
    expect(auth.permissionsStatus).toBe('ready')
    expect(auth.accessToken).toBe('A2')
    expect(auth.isAuthenticated).toBe(true)
  })

  it('طلبان retry متزامنان يشتركان في refresh واحد', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'RA' }))
    auth.markPermissionsLoading()

    postRefreshToken.mockRejectedValueOnce(networkError())
    await expect(bootstrapSession()).resolves.toBe(false)
    expect(postRefreshToken).toHaveBeenCalledTimes(1)

    const gate = deferred<AuthLoginSuccessResponse>()
    postRefreshToken.mockImplementation(() => gate.promise)

    const r1 = retrySessionBootstrap()
    const r2 = retrySessionBootstrap()

    await vi.waitFor(() => expect(postRefreshToken).toHaveBeenCalledTimes(2))

    gate.resolve(authPayload({ accessToken: 'A2', refreshToken: 'RA2' }))

    await expect(r1).resolves.toBe(true)
    await expect(r2).resolves.toBe(true)
    expect(postRefreshToken).toHaveBeenCalledTimes(2)
    expect(auth.permissionsStatus).toBe('ready')
  })

  it('رفض السيرفر للتوكن ينهي الجلسة ولا يُعاد إلى ما لا نهاية', async () => {
    const auth = useAuthStore()
    auth.replaceSession(authPayload({ accessToken: 'A', refreshToken: 'BAD' }))
    auth.markPermissionsLoading()

    postRefreshToken.mockRejectedValueOnce(invalidRefreshError())

    await expect(bootstrapSession()).resolves.toBe(false)
    expect(auth.isAuthenticated).toBe(false)
    expect(auth.refreshToken).toBeFalsy()

    await expect(retrySessionBootstrap()).resolves.toBe(false)
    // لا طلب HTTP إضافي بعد إنهاء الجلسة
    expect(postRefreshToken).toHaveBeenCalledTimes(1)
  })
})
