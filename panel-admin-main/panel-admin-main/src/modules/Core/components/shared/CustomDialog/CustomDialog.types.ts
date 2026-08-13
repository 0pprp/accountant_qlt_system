export interface Props {
  title: string
  modelValue?: boolean
}

export interface Emits {
  (event: 'update:isOpen', value: boolean): void
}
