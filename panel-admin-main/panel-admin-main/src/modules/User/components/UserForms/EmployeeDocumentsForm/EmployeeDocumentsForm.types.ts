import type { FormMode } from '../../UserTabs/UserTabs.types'
import type { Attachment } from '@/modules/User/types/model'

export interface Props {
  userId: number
  attachments?: Array<Attachment>
  userFormMode?: FormMode
}
