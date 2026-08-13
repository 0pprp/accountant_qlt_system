import type { User } from '../../types/model'

export interface Emits {
  (event: 'editItem', values: User): void
}
