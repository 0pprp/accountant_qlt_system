import type { Branch } from '../../types/model'

export interface Emits {
  (event: 'editItem', values: Branch): void
}
