export interface Props {
  modelValue?: string
  error?: string
  placeholder: string
  label: string
  required?: boolean
}

export interface Emits {
  (e: 'update:modelValue', value: string): void
}
