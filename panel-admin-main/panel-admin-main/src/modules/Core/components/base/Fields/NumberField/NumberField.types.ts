export interface Props {
  modelValue?: number | null
  error?: string
  placeholder: string
  label: string
  required?: boolean
  disabled?: boolean
  readonly?: boolean
  suffix?: string
  isPercent?: boolean
}

export interface Emits {
  (e: 'update:modelValue', value: number | null): void
}
