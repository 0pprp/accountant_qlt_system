export interface Props {
  modelValue?: number | null
  error?: string
  label?: string
  placeholder: string
  required?: boolean
  disabled?: boolean
  clearable?: boolean
  readonly?: boolean
  canCreate?: boolean
}

export interface Emits {
  (event: 'update:modelValue', value: number | null): void
}
