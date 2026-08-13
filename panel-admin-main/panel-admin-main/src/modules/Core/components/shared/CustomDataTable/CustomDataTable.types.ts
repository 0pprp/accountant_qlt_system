import type { Branch } from '@/modules/Branch/types/model'
import type {
  Order,
  OrderInstallmentPayment,
} from '@/modules/Order/types/model'
import type { OrderListReport, Payment } from '@/modules/Payment/types/model'
import type { Sale } from '@/modules/Sale/types/model'
import type { User, UserDailyReport } from '@/modules/User/types/model'
import type { Customer } from '@/modules/Customer/types/model'
import type { SafeSellers, SafeTransactions } from '@/modules/Safe/types/model'
import type { Product } from '@/modules/Warehouse/types/model'
import type { DashboardFinancialRow } from '@/modules/Core/types/model/dashboard'
import type { ActivityLogListItem } from '@/modules/ActivityLog/types/model'
import type { Notification } from '@/modules/Notification/types/model'

export type AllowedTypes =
  | DashboardFinancialRow
  | Branch
  | Sale
  | Order
  | User
  | Payment
  | Customer
  | SafeSellers
  | SafeTransactions
  | OrderListReport
  | Product
  | OrderInstallmentPayment
  | UserDailyReport
  | ActivityLogListItem
  | Notification

export interface Props<T extends AllowedTypes = AllowedTypes> {
  data: T[]
  columns: Column<T>[]
  loading?: boolean

  selectable?: boolean
  expandable?: boolean
  expansionLoading?: boolean

  showFlag?: boolean
  showRowNumbers?: boolean
  hideHeaders?: boolean

  totalRecords?: number
  pageSize: number
  paginatorEnabled?: boolean
  paginatorLabel?: string
  paginatorCount?: number | string

  tableStyle?: string
  tableClasses?: string
  responsiveLayout?: string

  dataKey?: string
}

export interface Column<T extends AllowedTypes = AllowedTypes> {
  field: Extract<keyof T, string> | (string & {})
  header: string
  sortable?: boolean
  type?: 'text' | 'number' | 'date' | 'currency' | 'boolean' | 'custom'
  style?: string
  headerStyle?: string
  formatter?: (value: T[keyof T] | unknown) => string
}

export interface Emits<T extends AllowedTypes = AllowedTypes> {
  pageChange: [event: { first: number; page: number; rows: number }]
  rowExpand: [event: { data: T }]
  rowCollapse: [event: { data: T }]
  selectionChange: [selectedItems: T[]]
  sortChange: [event: { sortField: string | null; sortOrder: 1 | -1 | 0 }]
}
