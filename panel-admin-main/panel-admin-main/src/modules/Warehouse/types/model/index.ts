export interface ProductCategory {
  id: number
  name: string
  productsCount: number
}

export interface Product {
  id: number
  name: string
  remainingCount: number
  buyAmount: number
  sellAmount: number
  dailyInstallmentAmount: number
  description: null
  categoryName: string
  warehouseName: string | null
  createdAt: string
  creatorName: string
}

export interface Warehouse {
  id: number
  name: string
}

export interface ProductCategoryForm {
  name: string
}

export interface ProductForm {
  name: string
  remainingCount: number
  buyAmount: number
  sellAmount: number
  dailyInstallmentAmount: number
  description: string | null
  categoryId: number
  warehouseId: number
  branchId: number
}

export interface AllProductModel {
  id: number
  name: string
  buyAmount: number
  dailyInstallmentAmount: number
  sellAmount: number
}
