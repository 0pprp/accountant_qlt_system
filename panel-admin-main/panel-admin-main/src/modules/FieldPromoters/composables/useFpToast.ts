import { useToastStore } from '@/modules/Core/store'
import { DialogState } from '@/modules/Core/types/model/dialog'

interface FpToastMessage {
  severity: 'success' | 'error' | 'info' | 'warn'
  summary: string
  detail?: string
  life?: number
}

export function useFpToast() {
  const store = useToastStore()
  return {
    add(message: FpToastMessage) {
      store.setMassage({
        title: message.summary,
        description: message.detail,
        dialogState: message.severity === 'error' ? DialogState.Error : DialogState.Success,
        isOpen: true,
      })
    },
  }
}
