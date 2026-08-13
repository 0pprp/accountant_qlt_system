export interface Props {
  modelValue?: number | null
  label?: string
  placeholder?: string
  required?: boolean
  readonly?: boolean
  disabled?: boolean
  error?: string
  customerId?: number | null
  inProgressOnly?: boolean
}

export interface Emits {
  (e: 'update:modelValue', value: number | null): void
}
