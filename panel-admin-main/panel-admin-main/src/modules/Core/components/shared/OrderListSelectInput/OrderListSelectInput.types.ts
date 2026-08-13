export interface Props {
  modelValue?: number | Array<number> | null
  error?: string
  label: string
  multiple?: boolean
  placeholder: string
  required?: boolean
  disabled?: boolean
  clearable?: boolean
  readonly?: boolean
}

export interface Emits {
  (event: 'update:modelValue', value: number | Array<number> | null): void
}
