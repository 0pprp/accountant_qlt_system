export type ExcelReportParamValue =
  | string
  | number
  | boolean
  | Date
  | string[]
  | number[]
  | null
  | undefined

export interface Props {
  endpoint: string
  filename?: string
  params?: Record<string, ExcelReportParamValue>
  label?: string
  disabled?: boolean
}
