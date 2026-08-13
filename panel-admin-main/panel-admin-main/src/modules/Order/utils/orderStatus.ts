import {
  OrderApprovalStatus,
  OrderExecutionStatus,
  OrderStep,
} from '@/modules/Order/types/model'

export function migrateColumnKey(key: string): string {
  return key === 'Status' ? 'ExecutionStatus' : key
}

export function migrateColumnKeys(keys: Array<string>): Array<string> {
  return keys.map(migrateColumnKey)
}

function getRowFieldValue(
  item: Record<string, unknown>,
  field: string
): unknown {
  if (field in item && item[field] != null && item[field] !== '') {
    return item[field]
  }

  const lowerField = field.toLowerCase()
  for (const key in item) {
    if (
      key.toLowerCase() === lowerField &&
      item[key] != null &&
      item[key] !== ''
    ) {
      return item[key]
    }
  }

  return undefined
}

export function getExecutionStatusFromRow(
  item: Record<string, unknown>
): OrderExecutionStatus | undefined {
  const raw =
    item.executionStatus ?? item.ExecutionStatus ?? item.status ?? item.Status

  if (raw === null || raw === undefined || raw === '') {
    return undefined
  }

  const numericValue = Number(raw)
  if (Number.isNaN(numericValue)) {
    return undefined
  }

  return numericValue as OrderExecutionStatus
}

export function getApprovalStatusFromRow(
  item: Record<string, unknown>
): OrderApprovalStatus | undefined {
  const raw = getRowFieldValue(item, 'ApprovalStatus')

  if (raw === null || raw === undefined || raw === '') {
    return undefined
  }

  const numericValue = Number(raw)
  if (Number.isNaN(numericValue)) {
    return undefined
  }

  return numericValue as OrderApprovalStatus
}

export function getOrderStepFromRow(
  item: Record<string, unknown>
): OrderStep | undefined {
  const raw = getRowFieldValue(item, 'Step')

  if (raw === null || raw === undefined || raw === '') {
    return undefined
  }

  const numericValue = Number(raw)
  if (Number.isNaN(numericValue)) {
    return undefined
  }

  return numericValue as OrderStep
}

type TranslateFn = (key: string) => string

export function getApprovalStatusLabel(
  status: OrderApprovalStatus | undefined,
  t: TranslateFn
): string {
  switch (status) {
    case OrderApprovalStatus.Pending:
      return t('sale.approvalStatus.pending')
    case OrderApprovalStatus.Approved:
      return t('sale.approvalStatus.approved')
    case OrderApprovalStatus.Rejected:
      return t('sale.approvalStatus.rejected')
    default:
      return '-'
  }
}

export function getExecutionStatusLabel(
  status: OrderExecutionStatus | undefined,
  t: TranslateFn
): string {
  switch (status) {
    case OrderExecutionStatus.NotStarted:
      return t('sale.executionStatus.notStarted')
    case OrderExecutionStatus.InProgress:
      return t('sale.executionStatus.inProgress')
    case OrderExecutionStatus.Completed:
      return t('sale.executionStatus.completed')
    default:
      return '-'
  }
}

export function formatApprovalStatusCell(
  value: unknown,
  t: TranslateFn
): string {
  if (value === null || value === undefined || value === '-') {
    return '-'
  }

  const numericValue = Number(value)
  if (!Number.isNaN(numericValue)) {
    return getApprovalStatusLabel(numericValue as OrderApprovalStatus, t)
  }

  return String(value)
}

export function formatExecutionStatusCell(
  value: unknown,
  t: TranslateFn
): string {
  if (value === null || value === undefined || value === '-') {
    return '-'
  }

  const numericValue = Number(value)
  if (!Number.isNaN(numericValue)) {
    return getExecutionStatusLabel(numericValue as OrderExecutionStatus, t)
  }

  return String(value)
}

export function canChangeOrderApproval(item: Record<string, unknown>): boolean {
  return getApprovalStatusFromRow(item) === OrderApprovalStatus.Pending
}

export function isOrderInProgress(item: Record<string, unknown>): boolean {
  return getExecutionStatusFromRow(item) === OrderExecutionStatus.InProgress
}
