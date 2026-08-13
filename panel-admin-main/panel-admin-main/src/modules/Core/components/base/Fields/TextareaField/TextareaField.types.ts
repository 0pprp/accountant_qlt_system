export interface Props {
  modelValue?: string | null
  error?: string
  placeholder: string
  label: string
  required?: boolean
  readonly?: boolean
  rows: number
}

export interface Emits {
  (e: 'update:modelValue', value: string): void
}
