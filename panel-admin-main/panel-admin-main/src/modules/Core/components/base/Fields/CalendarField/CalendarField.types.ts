export interface Props {
  modelValue?: string | null
  error?: string
  placeholder: string
  required?: boolean
  label: string
  readonly?: boolean
  timeOnly?: boolean
}

export interface Emits {
  (e: 'update:modelValue', value: string | null): void
}
