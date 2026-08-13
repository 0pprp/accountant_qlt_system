import type { TransferForm } from '../../types/model'

export interface Props {
  loading?: boolean
}

export interface Emits {
  (event: 'submit', value: TransferForm): void
}
