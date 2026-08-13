import type {
  AllProductModel,
  Product,
  ProductCategory,
  ProductCategoryForm,
  ProductForm,
} from '../model'

export type ProductCategoryCreatePayload = ProductCategoryForm

export type ProductCategoryListServerSuccessResponse = {
  items: ServerSuccessPaginatedResponse<Array<ProductCategory>>
  totalProductsCount: number
}

export type ProductCreatePayload = ProductForm

export type ProductListServerSuccessResponse = {
  paginatedProducts: ServerSuccessPaginatedResponse<Array<Product>>
  totalBuyAmount: number
  totalSellAmount: number
  totalRemainingCount: number
}

export type AllProductListServerSuccessResponse = {
  paginatedProducts: ServerSuccessPaginatedResponse<Array<AllProductModel>>
  totalBuyAmount: number
  totalSellAmount: number
}
