import type { Business } from '@/modules/Customer/types/model'
import type { FormMode } from '@/modules/User/components/UserTabs/UserTabs.types'
import type { Attachment } from '@/modules/User/types/model'

export interface Props {
  fullName?: string
  motherName?: string
  nationalCode?: string
  birthDate?: string
  phoneNumber?: string
  whatsAppPhoneNumber?: string
  business?: Business

  customerFormMode: FormMode
}

export interface Emits {
  (e: 'update:attachments', value: Array<Attachment>, customerId: number): void
}
