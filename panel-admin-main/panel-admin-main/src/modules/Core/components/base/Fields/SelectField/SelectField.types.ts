export interface Props {
  modelValue?: number | null
  options: Array<SelectOption>
  error?: string
  placeholder: string
  required?: boolean
  loading?: boolean
  label: string
  readonly?: boolean
  clearable?: boolean
}

export interface SelectOption {
  label: string
  value: number | null
}

export interface Emits {
  (e: 'update:modelValue', value: number | null): void
}
