export interface Props {
  modelValue?: number | null
  error?: string
  label: string
  placeholder: string
  required?: boolean
  disabled?: boolean
  clearable?: boolean
  readonly?: boolean
  canCreate?: boolean
  /** Shown when there is no productId (e.g. foreign purchase line) */
  displayName?: string | null
}

export interface Emits {
  (event: 'update:modelValue', value: number | null): void
  (event: 'update:productName', value: string): void
  (event: 'update:amount', value: number): void
  (event: 'update:dailyInstallmentAmount', value: number): void
  (event: 'update:sellAmount', value: number): void
}
