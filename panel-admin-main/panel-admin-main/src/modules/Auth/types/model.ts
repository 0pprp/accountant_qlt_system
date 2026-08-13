export interface AuthLoginForm {
  username: string
  password: string
}

export interface AuthTokens {
  accessToken: string
  refreshToken: string
}

export interface Permissions {
  name: PermissionAction
  id: number
}
export interface RoutePermissions {
  module: PermissionModule
  action: PermissionAction
}

export type PermissionAction =
  | 'Create'
  | 'Read'
  | 'Update'
  | 'Delete'
  | 'Collect'
  | 'CollectCash'

export type PermissionModule =
  | 'Attachment'
  | 'Branch'
  | 'Province'
  | 'InstallmentPayment'
  | 'Order'
  | 'OrderList'
  | 'Product'
  | 'ProductCategory'
  | 'Role'
  | 'User'
  | 'Warehouse'
  | 'Permission'
  | 'Customer'
  | 'Purchase'
  | 'Expense'
  | 'Safe'
  | 'Transaction'
  | 'ActivityLog'
  | 'Notification'

export interface ModulePermissions {
  Create?: boolean
  Read?: boolean
  Update?: boolean
  Delete?: boolean
  Collect?: boolean
  CollectCash?: boolean
}

export interface PermissionTypes {
  Attachment?: Permissions[]
  Branch?: Permissions[]
  Province?: Permissions[]
  InstallmentPayment?: Permissions[]
  Order?: Permissions[]
  OrderList?: Permissions[]
  Product?: Permissions[]
  ProductCategory?: Permissions[]
  Role?: Permissions[]
  User?: Permissions[]
  BusinessInformation?: Permissions[]
  Warehouse?: Permissions[]
  Permission?: Permissions[]
  Customer?: Permissions[]
  Purchase?: Permissions[]
  Expense?: Permissions[]
  Safe?: Permissions[]
  Transaction?: Permissions[]
  ActivityLog?: Permissions[]
  Notification?: Permissions[]
}

export interface ConvertedPermissionTypes {
  Attachment?: ModulePermissions
  Branch?: ModulePermissions
  Province?: ModulePermissions
  InstallmentPayment?: ModulePermissions
  Order?: ModulePermissions
  OrderList?: ModulePermissions
  Product?: ModulePermissions
  ProductCategory?: ModulePermissions
  Role?: ModulePermissions
  User?: ModulePermissions
  BusinessInformation?: ModulePermissions
  Warehouse?: ModulePermissions
  Permission?: ModulePermissions
  Customer?: ModulePermissions
  Purchase?: ModulePermissions
  Expense?: ModulePermissions
  Safe?: ModulePermissions
  Transaction?: ModulePermissions
  ActivityLog?: ModulePermissions
  Notification?: ModulePermissions
}
