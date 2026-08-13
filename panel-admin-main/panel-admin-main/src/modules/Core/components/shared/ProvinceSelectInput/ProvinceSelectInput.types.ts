export interface Props {
  modelValue?: number | null
  error?: string
  label: string
  placeholder: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  clearable?: boolean
}

export interface Emits {
  (event: 'update:modelValue', value: number | null): void
}
