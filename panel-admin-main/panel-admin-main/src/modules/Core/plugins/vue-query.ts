import type { App } from 'vue'
import {
  MutationCache,
  QueryClient,
  VueQueryPlugin,
  type VueQueryPluginOptions,
} from '@tanstack/vue-query'
import { useToastStore } from './../store/index'
import { DialogState } from '../types/model/dialog'
import { isAbortError } from '@/modules/Auth/session/sessionAbort'

interface Meta {
  noSuccessToast?: boolean
  success?: {
    title: string
    description?: string
  }
  error?: {
    title: string
    description?: string
  }
}

function createMutationCache(toastStore: ReturnType<typeof useToastStore>) {
  return new MutationCache({
    onSuccess(_, __, ___, { options }) {
      const meta = options.meta as Meta
      if (meta?.noSuccessToast) {
        return
      }

      if (meta?.success) {
        toastStore.clearToast()
        toastStore.setMassage({
          title: meta?.success.title,
          description: meta?.success.description,
          dialogState: DialogState.Success,
          isOpen: true,
        })
        return
      }
    },
    onError(err, _variables, _context, { options }) {
      if (isAbortError(err)) return

      const error = err as AxiosCustomError
      const meta = options.meta as Meta

      if (!error.response) {
        return
      }

      if (meta?.error) {
        toastStore.clearToast()
        toastStore.setMassage({
          title: meta?.error.title,
          description: meta?.error.description,
          dialogState: DialogState.Error,
          isOpen: true,
        })
      }
    },
  })
}

let queryClient: QueryClient | null = null

export function getQueryClient(): QueryClient {
  if (!queryClient) {
    throw new Error('QueryClient لم يُهيَّأ بعد — استدعِ vueQuery أولًا')
  }
  return queryClient
}

/** إنهاء جلسة / تبديل حساب */
export function clearSessionQueryCache() {
  if (!queryClient) return
  void queryClient.cancelQueries()
  queryClient.clear()
}

/**
 * تبديل فرع — إلغاء الجاري ومسح الكاش حتى لا تظهر بيانات الفرع السابق
 * إذا وصلت استجابة متأخرة أو بقي مفتاح query مشترك.
 */
export function cancelBranchQueries() {
  if (!queryClient) return
  void queryClient.cancelQueries()
  queryClient.clear()
}

/**
 * إبطال بيانات قد تصبح غير مسموحة بعد تغيّر الصلاحيات/الفروع.
 * لا يلغي طلبًا ينتظر إعادة المحاولة بعد refresh توكن فقط.
 */
export function invalidateSessionQueries() {
  if (!queryClient) return
  void queryClient.invalidateQueries()
}

export function vueQuery(app: App) {
  const toastStore = useToastStore()

  queryClient = new QueryClient({
    mutationCache: createMutationCache(toastStore),
    defaultOptions: {
      queries: {
        retry: 0,
        refetchOnWindowFocus: false,
        refetchOnMount: true,
        staleTime: 0,
      },
    },
  })

  const options: VueQueryPluginOptions = { queryClient }

  app.use(VueQueryPlugin, options)
}
