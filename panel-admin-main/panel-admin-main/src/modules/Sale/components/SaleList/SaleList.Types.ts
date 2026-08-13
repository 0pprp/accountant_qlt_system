import type { Sale } from '../../types/model'

export interface Emits {
  (event: 'editItem', values: Sale): void
}
