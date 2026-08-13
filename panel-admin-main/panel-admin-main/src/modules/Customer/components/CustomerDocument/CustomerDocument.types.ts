import { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types.ts'
import type { Attachment } from '@/modules/User/types/model'

export interface Props {
  customerId: number
  attachments?: Array<Attachment>
  formMode?: FormMode
}
