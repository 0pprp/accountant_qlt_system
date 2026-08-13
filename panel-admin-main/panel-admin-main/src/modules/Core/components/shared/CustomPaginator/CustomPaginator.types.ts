export interface Props {
  rows?: number
  totalRecords: number
  rowsPerPageOptions?: number[]
  label?: string
  modelValue?: number
}

export interface Emits {
  (e: 'update:modelValue', value: number): void
  (e: 'pageChange', event: { first: number; page: number; rows: number }): void
}
