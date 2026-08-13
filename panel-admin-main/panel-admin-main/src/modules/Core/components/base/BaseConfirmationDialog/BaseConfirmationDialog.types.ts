export interface Props {
  title: string
  submitText: string
  modelValue?: boolean
  width?: string
  icon: string
  color: string
  disabled?: boolean
}

export interface Emits {
  (event: 'update:isOpen', value: boolean): void
  (event: 'onSubmit'): void
}
