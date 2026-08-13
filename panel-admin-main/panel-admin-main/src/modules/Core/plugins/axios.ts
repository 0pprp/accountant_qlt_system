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
import {
  REFRESH_TOKEN_ENDPOINT,
  refreshMutation,
} from '@/modules/Auth/requests/mutations'

const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

let isRefreshing = false
let failedQueue: {
  resolve: (value: AxiosResponse) => void
  reject: (error: AxiosError) => void
  config: InternalAxiosRequestConfig
}[] = []

function processRequestsQueue(error: AxiosError | null, token?: string) {
  failedQueue.forEach(({ resolve, reject, config }) => {
    if (error) {
      reject(error)
    } else {
      config.headers = config.headers || {}
      config.headers['Authorization'] = `Bearer ${token}`

      axiosInstance(config).then(resolve).catch(reject)
    }
  })

  failedQueue = []
}

function onRequest(config: InternalAxiosRequestConfig) {
  const authStore = useAuthStore()
  const token = authStore?.accessToken

  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`
  }

  return config
}

function onResponse(res: AxiosResponse) {
  return res
}

async function onResponseError(error: AxiosError<ErrorData>) {
  const toastStore = useToastStore()
  const { response, config } = error

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

  const originalRequest = config as InternalAxiosRequestConfig & {
    retry?: boolean
  }

  if (originalRequest.retry) {
    return Promise.reject(error)
  }

  const authStore = useAuthStore()

  originalRequest.retry = true

  if (originalRequest.url?.includes(REFRESH_TOKEN_ENDPOINT)) {
    authStore.$reset()
    router.push('/auth')

    return Promise.reject(error)
  }

  if (isRefreshing) {
    return new Promise<AxiosResponse>((resolve, reject) =>
      failedQueue.push({ resolve, reject, config: originalRequest })
    )
  }

  isRefreshing = true

  try {
    const { data } = await refreshMutation({
      refreshToken: authStore.refreshToken,
    })

    authStore.setToken(data)
    const newToken = data.accessToken

    processRequestsQueue(null, newToken)

    originalRequest.headers = originalRequest.headers || {}
    originalRequest.headers['Authorization'] = `Bearer ${newToken}`

    return axiosInstance(originalRequest)
  } catch (err) {
    processRequestsQueue(err as AxiosError)

    useAuthStore().$reset()
    router.push('/auth')

    return Promise.reject(err)
  } finally {
    isRefreshing = false
  }
}

axiosInstance.interceptors.request.use(onRequest)
axiosInstance.interceptors.response.use(onResponse, onResponseError)

export { axiosInstance, registerAxios }

function registerAxios(app: App) {
  app.config.globalProperties.$axios = axiosInstance
}
