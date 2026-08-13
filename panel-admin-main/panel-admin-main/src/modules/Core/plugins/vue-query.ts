import type { App } from 'vue'
import {
  MutationCache,
  QueryClient,
  VueQueryPlugin,
  type VueQueryPluginOptions,
} from '@tanstack/vue-query'
import { useToastStore } from './../store/index'
import { DialogState } from '../types/model/dialog'

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

export function vueQuery(app: App) {
  const toastStore = useToastStore()

  const mutationCache = new MutationCache({
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

  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: 0,
        refetchOnWindowFocus: false,
        refetchOnMount: true,
        staleTime: 0,
      },
    },
    mutationCache,
  })

  const options: VueQueryPluginOptions = { queryClient }

  app.use(VueQueryPlugin, options)
}
