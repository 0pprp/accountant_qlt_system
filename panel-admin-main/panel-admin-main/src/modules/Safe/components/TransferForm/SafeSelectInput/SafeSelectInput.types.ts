import type { Safe } from '@/modules/Safe/types/model'

export interface Props {
  hasForeignSafe?: boolean
  hasMainSafe?: boolean
  modelValue?: number | null
  error?: string
  label: string
  placeholder: string
  required?: boolean
  disabled?: boolean
  clearable?: boolean
  readonly?: boolean
  items?: Safe[]
}

export interface Emits {
  (event: 'update:modelValue', value: number | null): void
}
