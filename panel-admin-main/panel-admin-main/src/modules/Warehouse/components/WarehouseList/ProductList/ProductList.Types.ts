import type { Product } from '../../../types/model'

export interface Props {
  products?: Product[]
}

export interface Emits {
  (e: 'updateProduct', product: Product): void
  (e: 'deleteProduct', product: Product): void
}
