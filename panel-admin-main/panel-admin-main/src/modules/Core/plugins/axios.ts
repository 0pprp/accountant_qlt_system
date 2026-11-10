import type {
  AxiosError,
  AxiosResponse,
  InternalAxiosRequestConfig,
} from 'axios'
import axios from 'axios'
import type { App } from 'vue'
import { type ErrorData } from '../types/model/toast'
import { useToastStore } from '../store'
import { getLocalizationTitle } from '../utils'
import { DialogState } from '../types/model/dialog'
import router from '@/router'
import useAuthStore from '@/modules/Auth/store'
import { REFRESH_TOKEN_ENDPOINT } from '@/modules/Auth/requests/mutations/refreshMutation'
import {
  getSessionAbortSignal,
  isAbortError,
  mergeAbortSignals,
} from '@/modules/Auth/session/sessionAbort'
import { getBranchAbortSignal } from '@/modules/Auth/session/branchAbort'
import { isStaleGeneration } from '@/modules/Auth/session/sessionGeneration'
import {
  abortRefreshForGeneration,
  hasActiveRefreshForGeneration,
  refreshSessionForGeneration,
} from '@/modules/Auth/session/sessionRefreshCoordinator'
import { onSessionEnded } from '@/modules/Auth/session/sessionHttpBridge'

export type SessionAxiosConfig = InternalAxiosRequestConfig & {
  /** يُلتقط عند إنشاء الطلب ولا يُستبدل بجيل لاحق */
  sessionGeneration?: number
  /** جيل فرع الحساب عند إنشاء الطلب — يرفض الاستجابة إن تبدّل الفرع */
  branchGeneration?: number
  retry?: boolean
  skipAuthRefresh?: boolean
}

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

type QueueItem = {
  resolve: (value: AxiosResponse) => void
  reject: (error: AxiosError) => void
  config: SessionAxiosConfig
  sessionGeneration: number
}

let refreshOwnerGeneration: number | null = null
let failedQueue: QueueItem[] = []

function createStaleSessionError(): AxiosError {
  return Object.assign(new Error('stale_session'), {
    code: 'ERR_CANCELED',
    name: 'CanceledError',
    isAxiosError: true,
    toJSON: () => ({}),
  }) as AxiosError
}

function isStaleRequest(
  requestGeneration: number | undefined,
  currentGeneration: number
): boolean {
  return (
    requestGeneration != null &&
    isStaleGeneration(requestGeneration, currentGeneration)
  )
}

function rejectQueueForGeneration(
  generation: number,
  error: AxiosError = createStaleSessionError()
) {
  const remaining: QueueItem[] = []
  for (const item of failedQueue) {
    if (item.sessionGeneration === generation) {
      item.reject(error)
    } else {
      remaining.push(item)
    }
  }
  failedQueue = remaining
}

function rejectAllQueued(error: AxiosError) {
  failedQueue.forEach(({ reject }) => reject(error))
  failedQueue = []
}

function processQueueAfterRefresh(token: string, refreshGeneration: number) {
  const authStore = useAuthStore()
  const snapshot = failedQueue
  failedQueue = []

  for (const item of snapshot) {
    if (isStaleGeneration(item.sessionGeneration, authStore.sessionGeneration)) {
      item.reject(createStaleSessionError())
      continue
    }
    if (item.sessionGeneration !== refreshGeneration) {
      // طابور جيل مختلف — لا يُعاد بتوكن هذه العملية
      item.reject(createStaleSessionError())
      continue
    }

    item.config.headers = item.config.headers || {}
    item.config.headers.Authorization = `Bearer ${token}`
    // حافظ على جيل الطلب الأصلي عند إعادة الإرسال
    item.config.sessionGeneration = item.sessionGeneration
    axiosInstance(item.config).then(item.resolve).catch(item.reject)
  }
}

function onRequest(config: InternalAxiosRequestConfig) {
  const authStore = useAuthStore()
  const sessionConfig = config as SessionAxiosConfig

  // التقاط الجيل مرة واحدة عند الإنشاء — لا تستبدله عند إعادة الإرسال
  if (sessionConfig.sessionGeneration == null) {
    sessionConfig.sessionGeneration = authStore.sessionGeneration
  }
  if (sessionConfig.branchGeneration == null) {
    sessionConfig.branchGeneration = authStore.branchGeneration
  }

  const requestGen = sessionConfig.sessionGeneration

  // لا تضع توكن حساب آخر على طلب قديم
  if (
    !isStaleRequest(requestGen, authStore.sessionGeneration) &&
    authStore.accessToken
  ) {
    sessionConfig.headers = sessionConfig.headers || {}
    sessionConfig.headers.Authorization = `Bearer ${authStore.accessToken}`
  }

  sessionConfig.signal = mergeAbortSignals(
    getSessionAbortSignal(),
    mergeAbortSignals(
      getBranchAbortSignal(),
      sessionConfig.signal as AbortSignal | undefined
    )
  )

  return sessionConfig
}

function onResponse(res: AxiosResponse) {
  const authStore = useAuthStore()
  const cfg = res.config as SessionAxiosConfig
  const requestGen = cfg.sessionGeneration

  if (isStaleRequest(requestGen, authStore.sessionGeneration)) {
    return Promise.reject(createStaleSessionError())
  }
  if (isStaleRequest(cfg.branchGeneration, authStore.branchGeneration)) {
    return Promise.reject(createStaleSessionError())
  }

  return res
}

async function onResponseError(error: AxiosError<ErrorData>) {
  if (isAbortError(error)) {
    return Promise.reject(error)
  }

  const authStore = useAuthStore()
  const toastStore = useToastStore()
  const { response, config } = error
  const sessionConfig = (config ?? {}) as SessionAxiosConfig
  const requestGen = sessionConfig.sessionGeneration

  // طلب سابق بعد تبديل الحساب أو الفرع — إلغاء صامت
  if (isStaleRequest(requestGen, authStore.sessionGeneration)) {
    return Promise.reject(createStaleSessionError())
  }
  if (isStaleRequest(sessionConfig.branchGeneration, authStore.branchGeneration)) {
    return Promise.reject(createStaleSessionError())
  }

  if (!response) {
    toastStore.setMassage({
      title: getLocalizationTitle('serveErrorMassage'),
      description: getLocalizationTitle('serveErrorDescription'),
      dialogState: DialogState.Error,
      isOpen: true,
    })
    throw error
  }

  if (response.status === 500) {
    toastStore.setMassage({
      title: getLocalizationTitle('serveErrorMassage'),
      description: getLocalizationTitle('serveErrorDescription'),
      dialogState: DialogState.Error,
      isOpen: true,
    })
    return Promise.reject(error)
  }

  if (response.status !== 401) {
    const errorMessage = response.data?.errors?.[0]?.message
    const errorTitle = errorMessage || getLocalizationTitle('serveErrorMassage')

    toastStore.setMassage({
      title: errorTitle,
      dialogState: DialogState.Error,
      isOpen: true,
    })

    return Promise.reject(error)
  }

  if (sessionConfig.retry) {
    return Promise.reject(error)
  }

  if (sessionConfig.skipAuthRefresh) {
    return Promise.reject(error)
  }

  if (sessionConfig.url?.includes(REFRESH_TOKEN_ENDPOINT)) {
    // فشل refresh نفسه — أنهِ فقط إن بقيت نفس الجلسة
    if (!isStaleRequest(requestGen, authStore.sessionGeneration)) {
      const endedGen = requestGen ?? authStore.sessionGeneration
      rejectQueueForGeneration(endedGen)
      authStore.endSession('refresh_failed')
      await router.push({ name: 'AuthLoginRoute' })
    }
    return Promise.reject(error)
  }

  const generationForRefresh = requestGen ?? authStore.sessionGeneration

  // لا تبدأ refresh لجيل قديم بعد دخول حساب جديد
  if (isStaleRequest(generationForRefresh, authStore.sessionGeneration)) {
    return Promise.reject(createStaleSessionError())
  }

  if (
    refreshOwnerGeneration != null &&
    refreshOwnerGeneration !== generationForRefresh
  ) {
    return Promise.reject(createStaleSessionError())
  }

  // انضم للطابور إن كان refresh لنفس الجيل جاريًا (مالك HTTP أو bootstrap)
  const refreshAlreadyRunning =
    refreshOwnerGeneration === generationForRefresh ||
    hasActiveRefreshForGeneration(generationForRefresh)

  if (refreshAlreadyRunning && refreshOwnerGeneration === generationForRefresh) {
    return new Promise<AxiosResponse>((resolve, reject) => {
      failedQueue.push({
        resolve,
        reject,
        config: sessionConfig,
        sessionGeneration: generationForRefresh,
      })
    })
  }

  // bootstrap يملك التنسيق — انضم لنفس الـ promise ثم أعد المحاولة دون تدوير ثانٍ
  if (refreshAlreadyRunning && refreshOwnerGeneration == null) {
    sessionConfig.retry = true
    try {
      const data = await refreshSessionForGeneration({
        generation: generationForRefresh,
        refreshToken: authStore.refreshToken,
      })
      if (isStaleGeneration(generationForRefresh, authStore.sessionGeneration)) {
        return Promise.reject(createStaleSessionError())
      }
      authStore.applyAuthPayload(data, generationForRefresh, {
        source: 'refresh',
      })
      sessionConfig.headers = sessionConfig.headers || {}
      sessionConfig.headers.Authorization = `Bearer ${data.accessToken}`
      sessionConfig.sessionGeneration = generationForRefresh
      return axiosInstance(sessionConfig)
    } catch (err) {
      return Promise.reject(err)
    }
  }

  refreshOwnerGeneration = generationForRefresh
  sessionConfig.retry = true

  const refreshTokenSnapshot = authStore.refreshToken

  try {
    const data = await refreshSessionForGeneration({
      generation: generationForRefresh,
      refreshToken: refreshTokenSnapshot,
    })

    if (isStaleGeneration(generationForRefresh, authStore.sessionGeneration)) {
      rejectQueueForGeneration(generationForRefresh)
      return Promise.reject(createStaleSessionError())
    }

    const applied = authStore.applyAuthPayload(data, generationForRefresh, {
      source: 'refresh',
    })
    if (!applied) {
      rejectQueueForGeneration(generationForRefresh)
      return Promise.reject(createStaleSessionError())
    }

    const newToken = data.accessToken
    processQueueAfterRefresh(newToken, generationForRefresh)

    sessionConfig.headers = sessionConfig.headers || {}
    sessionConfig.headers.Authorization = `Bearer ${newToken}`
    sessionConfig.sessionGeneration = generationForRefresh

    return axiosInstance(sessionConfig)
  } catch (err) {
    const axiosErr = (err as AxiosError) ?? error
    if (!isStaleGeneration(generationForRefresh, authStore.sessionGeneration)) {
      rejectQueueForGeneration(generationForRefresh, axiosErr)
      authStore.endSession('refresh_failed')
      await router.push({ name: 'AuthLoginRoute' })
    } else {
      // فشل refresh قديم — لا ترفض طابور الحساب الأحدث ولا تُنه جلسة B
      rejectQueueForGeneration(generationForRefresh, createStaleSessionError())
    }
    return Promise.reject(err)
  } finally {
    // لا تُسقط حالة عملية أحدث
    if (refreshOwnerGeneration === generationForRefresh) {
      refreshOwnerGeneration = null
    }
  }
}

axiosInstance.interceptors.request.use(onRequest)
axiosInstance.interceptors.response.use(onResponse, onResponseError)

onSessionEnded((endedGeneration) => {
  rejectQueueForGeneration(endedGeneration)
  abortRefreshForGeneration(endedGeneration)
  if (refreshOwnerGeneration === endedGeneration) {
    refreshOwnerGeneration = null
  }
})

export function resetAxiosRefreshStateForTests() {
  refreshOwnerGeneration = null
  failedQueue = []
}

export function getAxiosQueueSizeForTests() {
  return failedQueue.length
}

export function getRefreshOwnerGenerationForTests() {
  return refreshOwnerGeneration
}

export { axiosInstance, registerAxios }

function registerAxios(app: App) {
  app.config.globalProperties.$axios = axiosInstance
}
