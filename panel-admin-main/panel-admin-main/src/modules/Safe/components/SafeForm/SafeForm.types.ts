import type { SellerCashDeliveriesForm } from '../../types/model'

export interface Props {
  loading?: boolean
}

export interface Emits {
  (event: 'submit', value: SellerCashDeliveriesForm): void
}
