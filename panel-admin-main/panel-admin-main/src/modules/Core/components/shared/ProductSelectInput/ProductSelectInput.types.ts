export interface Props {
  modelValue?: number | null
  error?: string
  label: string
  placeholder: string
  required?: boolean
  disabled?: boolean
  clearable?: boolean
  readonly?: boolean
  productCategoryId: number | null
}

export interface Emits {
  (event: 'update:modelValue', value: number | null): void
  (event: 'update:productName', value: string): void
  (event: 'update:buyAmount', value: number): void
  (event: 'update:sellAmount', value: number): void
  (event: 'update:dailyInstallmentAmount', value: number): void
}
