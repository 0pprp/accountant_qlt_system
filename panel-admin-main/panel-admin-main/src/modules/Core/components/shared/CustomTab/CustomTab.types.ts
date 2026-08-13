import type { PermissionModule } from '@/modules/Auth/types/model'

export interface TabItem {
  title: string
  icon: string
  value: string | number
  enTitle?: string
  isLastTab: boolean
  permission?: PermissionModule
}

export interface Props {
  tabs: TabItem[]
  modelValue?: number | string
}

export interface Emits {
  (event: 'update:activeTab', value: number | string): void
}
