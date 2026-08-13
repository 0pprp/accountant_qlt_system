export interface Props {
  modelValue?: number | null
  error?: string
  label: string
  placeholder: string
  required?: boolean
  disabled?: boolean
  clearable?: boolean
  branchId?: number | null
  roleId?: Array<number> | null
  readonly?: boolean
  loading?: boolean
}

export interface Emits {
  (event: 'update:modelValue', value: number | null): void
}
