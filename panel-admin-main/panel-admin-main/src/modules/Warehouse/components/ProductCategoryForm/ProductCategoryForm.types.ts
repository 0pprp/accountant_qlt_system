import type { ProductCategory, ProductCategoryForm } from '../../types/model'

export interface Props {
  selectedProductCategory?: ProductCategory
  loading: boolean
}

export interface Emits {
  (event: 'submit', values: ProductCategoryForm): void
}
