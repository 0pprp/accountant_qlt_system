import type {
  PermissionAction,
  PermissionModule,
} from '@/modules/Auth/types/model'
import type { Warehouse } from '@/modules/Warehouse/types/model'

export interface SideBarItem {
  title: string
  value: string
  icon: string
  link?: string
  query?: Record<string, string>
  permission?: {
    module: PermissionModule
    action: PermissionAction
  }
  badgeCount?: number
}

export type SideBarList = Array<SideBarItem> | Array<Warehouse>
