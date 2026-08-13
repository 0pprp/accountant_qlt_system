import type { Product, ProductCategory } from '../../types/model'

export interface Emits {
  (e: 'updateProduct', product: Product): void
  (e: 'deleteProduct', product: Product): void
  (event: 'editItem', values: ProductCategory): void
}
