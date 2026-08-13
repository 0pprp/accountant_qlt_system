import type { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import type { Attachment } from '@/modules/User/types/model'

export interface Props {
  customerId: number
  attachments?: Array<Attachment>
  customerFormMode?: FormMode
}
