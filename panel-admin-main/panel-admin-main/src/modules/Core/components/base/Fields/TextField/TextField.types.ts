export interface Props {
  modelValue?: string | number
  error?: string
  placeholder: string
  required?: boolean
  readonly?: boolean
  disabled?: boolean
  label: string
}

export interface Emits {
  (e: 'update:modelValue', value: string | number): void
}
