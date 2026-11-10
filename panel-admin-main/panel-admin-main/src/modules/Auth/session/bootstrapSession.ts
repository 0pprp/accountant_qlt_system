import { useAuthStore } from '@/modules/Auth/store'
import { isStaleGeneration } from '@/modules/Auth/session/sessionGeneration'
import {
  isInvalidRefreshTokenError,
  refreshSessionForGeneration,
} from '@/modules/Auth/session/sessionRefreshCoordinator'

let bootstrapPromise: Promise<boolean> | null = null
let bootstrapGeneration: number | null = null

/**
 * التحقق من الجلسة المحفوظة عبر refresh المنسَّق.
 * يشترك مع interceptor — لا تدوير متزامن لنفس التوكن.
 */
export function bootstrapSession(): Promise<boolean> {
  const authStore = useAuthStore()
  const generation = authStore.sessionGeneration

  if (
    bootstrapPromise &&
    bootstrapGeneration === generation
  ) {
    return bootstrapPromise
  }

  bootstrapGeneration = generation
  bootstrapPromise = runBootstrap(generation).finally(() => {
    if (bootstrapGeneration === generation) {
      bootstrapPromise = null
      bootstrapGeneration = null
    }
  })

  return bootstrapPromise
}

async function runBootstrap(generationAtStart: number): Promise<boolean> {
  const authStore = useAuthStore()

  if (!authStore.refreshToken) {
    authStore.endSession('bootstrap_failed')
    return false
  }

  authStore.markPermissionsLoading()

  try {
    const data = await refreshSessionForGeneration({
      generation: generationAtStart,
      refreshToken: authStore.refreshToken,
    })

    if (isStaleGeneration(generationAtStart, authStore.sessionGeneration)) {
      return false
    }

    return authStore.applyAuthPayload(data, generationAtStart, {
      source: 'bootstrap',
    })
  } catch (error) {
    if (!isStaleGeneration(generationAtStart, authStore.sessionGeneration)) {
      // رفض نهائي من السيرفر → إنهاء الجلسة لتوجيه المستخدم لتسجيل الدخول
      if (isInvalidRefreshTokenError(error)) {
        authStore.endSession('bootstrap_failed')
      } else {
        // فشل مؤقت (شبكة وغيرها) — يسمح بـ retrySessionBootstrap
        authStore.markPermissionsError()
      }
    }
    return false
  }
}

export function retrySessionBootstrap(): Promise<boolean> {
  const authStore = useAuthStore()
  if (!authStore.refreshToken) {
    authStore.endSession('bootstrap_failed')
    return Promise.resolve(false)
  }
  // امسح promise القديم لنفس الجيل لإعادة المحاولة الصريحة
  bootstrapPromise = null
  bootstrapGeneration = null
  authStore.markPermissionsLoading()
  return bootstrapSession()
}

export function resetBootstrapStateForTests() {
  bootstrapPromise = null
  bootstrapGeneration = null
}
