export interface Props {
  modelValue?: boolean
  label: string
  iconName?: string
  color?: string
  disabled?: boolean
  readonly?: boolean
}

export interface Emits {
  (e: 'update:modelValue', value: boolean): void
}
