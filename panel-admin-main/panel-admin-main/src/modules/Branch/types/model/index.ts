export interface Branch {
  id: number
  name: string
  createAt: string
  province: Province
}

export interface Province {
  name: string
  id: number
}

export interface BranchForm {
  name: string
  provinceId: number
}

export interface BranchWarehouses {
  id: number
  name: string
}
