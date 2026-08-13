import type { Order } from '../../types/model'

export interface Emits {
  (event: 'editItem', values: Order): void
}
