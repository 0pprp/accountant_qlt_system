import type { PaymentCreatePayload } from '@/modules/Payment/types/api'

export interface Props {
  loading?: boolean
}

export interface Emits {
  (e: 'submit', values: PaymentCreatePayload): void
  (e: 'cancel'): void
}

