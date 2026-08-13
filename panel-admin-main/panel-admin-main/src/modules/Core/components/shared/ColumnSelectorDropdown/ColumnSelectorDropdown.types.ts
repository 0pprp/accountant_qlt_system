export interface ColumnSelectorOption {
  key: string
  displayName: string
}

export interface Props {
  options: Array<ColumnSelectorOption>
  labelKey: string
  placeholderKey: string
}
