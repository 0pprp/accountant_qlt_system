import type { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import type { Attachment } from '@/modules/User/types/model'

export interface Props {
  title?: string
  submitText?: string
  modelValue?: boolean
  width?: string
  icon?: string
  color?: string
  formMode?: FormMode
  existingFile?: Attachment
}

export interface Emits {
  (event: 'uploadFile', value: File): void
}
