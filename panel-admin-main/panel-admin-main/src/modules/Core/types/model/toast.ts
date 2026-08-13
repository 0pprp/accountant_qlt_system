import type { DialogState } from './dialog'

export interface ErrorData {
  status: number
  errors: ErrorModel[]
}

export interface ErrorModel {
  message: string
  code: string | null
}

export interface StatementDialog {
  title: string
  description?: string
  dialogState: DialogState
  isOpen: boolean
}
