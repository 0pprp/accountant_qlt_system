import { AxiosError } from 'axios'
import type { AuthLoginSuccessResponse } from '@/modules/Auth/types/api'
import { postRefreshToken } from '@/modules/Auth/session/refreshHttp'
import { isStaleGeneration } from '@/modules/Auth/session/sessionGeneration'
import { useAuthStore } from '@/modules/Auth/store'

type RefreshOutcome = 'pending' | 'success' | 'failure'

type RefreshEntry = {
  /** هوية العملية — التنظيف يطابقها حتى لا تمسح عملية قديمة عملية أحدث */
  id: symbol
  generation: number
  refreshToken: string
  promise: Promise<AuthLoginSuccessResponse>
  reject: (reason?: unknown) => void
  settled: boolean
  outcome: RefreshOutcome
}

let activeRefresh: RefreshEntry | null = null

/** توكنات رفضها السيرفر — لا تُعاد محاولتها إلى ما لا نهاية */
const invalidatedRefreshTokens = new Set<string>()

function createStaleError() {
  return Object.assign(new Error('stale_session'), {
    code: 'ERR_CANCELED',
    name: 'CanceledError',
  })
}

export function isInvalidRefreshTokenError(error: unknown): boolean {
  if (!error || typeof error !== 'object') return false
  const status =
    error instanceof AxiosError
      ? error.response?.status
      : (error as { response?: { status?: number } }).response?.status
  return status === 401 || status === 403
}

function clearEntryIfCurrent(entry: RefreshEntry) {
  if (activeRefresh?.id === entry.id) {
    activeRefresh = null
  }
}

/**
 * تنسيق refresh واحد لكل جيل جلسة أثناء التنفيذ.
 * النجاح بنفس التوكن قابل للمشاركة مع منضم متأخر.
 * الفشل المؤقت يُزال بهوية العملية — إعادة المحاولة ترسل HTTP جديدًا.
 */
export function refreshSessionForGeneration(input: {
  generation: number
  refreshToken: string
}): Promise<AuthLoginSuccessResponse> {
  if (invalidatedRefreshTokens.has(input.refreshToken)) {
    return Promise.reject(
      Object.assign(new Error('refresh_token_invalid'), {
        code: 'ERR_REFRESH_INVALID',
        name: 'RefreshTokenInvalidError',
        response: { status: 401 },
      })
    )
  }

  if (activeRefresh && activeRefresh.generation === input.generation) {
    // مشاركة العملية الجارية فقط
    if (!activeRefresh.settled) {
      return activeRefresh.promise
    }
    // نجاح سابق بنفس التوكن — انضمام متأخر دون طلب جديد
    if (
      activeRefresh.outcome === 'success' &&
      activeRefresh.refreshToken === input.refreshToken
    ) {
      return activeRefresh.promise
    }
    // فشل سابق أو توكن مختلف → عملية جديدة أدناه
  }

  let resolveFn: (value: AuthLoginSuccessResponse) => void
  let rejectFn: (reason?: unknown) => void
  const promise = new Promise<AuthLoginSuccessResponse>((resolve, reject) => {
    resolveFn = resolve
    rejectFn = reject
  })

  const entry: RefreshEntry = {
    id: Symbol('refresh'),
    generation: input.generation,
    refreshToken: input.refreshToken,
    promise,
    reject: (reason) => {
      if (!entry.settled) {
        entry.settled = true
        entry.outcome = 'failure'
        rejectFn!(reason)
        clearEntryIfCurrent(entry)
      }
    },
    settled: false,
    outcome: 'pending',
  }
  activeRefresh = entry

  void (async () => {
    try {
      const data = await postRefreshToken(input.refreshToken)
      if (entry.settled) return
      const authStore = useAuthStore()
      if (isStaleGeneration(input.generation, authStore.sessionGeneration)) {
        entry.settled = true
        entry.outcome = 'failure'
        rejectFn!(createStaleError())
        clearEntryIfCurrent(entry)
        return
      }
      entry.settled = true
      entry.outcome = 'success'
      resolveFn!(data)
      // أبقِ النجاح لنفس التوكن للمنضمين المتأخرين؛ لا تمسح هنا
    } catch (error) {
      if (entry.settled) return
      entry.settled = true
      entry.outcome = 'failure'
      if (isInvalidRefreshTokenError(error)) {
        invalidatedRefreshTokens.add(input.refreshToken)
      }
      rejectFn!(error)
      // إزالة العملية الفاشلة بهويتها فقط — لا تلمس عملية أحدث
      clearEntryIfCurrent(entry)
    }
  })()

  return promise
}

/** يرفض refresh الجيل المنتهي حتى لا تبقى وعود المالك معلّقة */
export function abortRefreshForGeneration(generation: number) {
  if (activeRefresh?.generation === generation) {
    const entry = activeRefresh
    entry.reject(createStaleError())
    clearEntryIfCurrent(entry)
  }
}

export function clearInvalidatedRefreshTokensForTests() {
  invalidatedRefreshTokens.clear()
}

export function getActiveRefreshGenerationForTests(): number | null {
  return activeRefresh?.generation ?? null
}

export function resetSessionRefreshCoordinatorForTests() {
  activeRefresh = null
  invalidatedRefreshTokens.clear()
}

export function hasActiveRefreshForGeneration(generation: number): boolean {
  return (
    activeRefresh?.generation === generation && activeRefresh.settled === false
  )
}
