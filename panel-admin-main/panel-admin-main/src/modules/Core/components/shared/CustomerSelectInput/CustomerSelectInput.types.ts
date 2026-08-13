export interface Props {
  modelValue?: number | Array<number> | null
  error?: string
  label: string
  multiple?: boolean
  placeholder: string
  required?: boolean
  loading?: boolean
  disabled?: boolean
  clearable?: boolean
  readonly?: boolean
}

export interface Emits {
  (event: 'update:modelValue', value: number): void
  (event: 'update:customerName', value: string): void
}
