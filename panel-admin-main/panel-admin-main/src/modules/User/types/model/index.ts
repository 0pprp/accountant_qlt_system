import { AttachmentType } from './../../../Core/types/model/attachment'
import type { Warehouse } from '@/modules/Warehouse/types/model'
import type { Role } from '@/modules/Role/types/model'
import type { PermissionAction } from '@/modules/Auth/types/model'
export interface User extends Record<string, unknown> {
  id: number
}

export enum UserSteps {
  accountDetails,
  employeeData,
  employeeDocuments,
  salaryDetails,
  power,
}

export interface AccountDetailsFormTypes {
  hasError: boolean
  password?: string
  userName: string
}

export interface EmployeeDataFormTypes {
  hasError: boolean
  fullName: string
  motherName: string
  nationalCode: string
  birthDate: string
  roleId: number
  branchIds: Array<number>
  address: string
  phoneNumber: string
}

export interface SalaryDetailsFormTypes {
  hasError: boolean
  type: SalaryType
  amount: number
  saleSharePercent: number | null
  installmentSharePercent: number | null
}

export interface UserForm
  extends AccountDetailsFormTypes,
    EmployeeDataFormTypes {}

export enum SalaryType {
  Fixed,
  CommissionBased,
}

export interface RolePermission {
  id: number
  name: PermissionAction
}

interface RolePermissions {
  [key: string]: RolePermission[]
}

export interface RolePermissionsResponse {
  permissions: RolePermissions
}
export interface Permission {
  id: number
  name: PermissionAction
  displayName: string
  scope: string
  scopeDisplayName: string
}
interface Permissions {
  [key: string]: Permission[]
}

export interface PermissionsResponse {
  permissions: Permissions
}

export interface UserPermissionForm {
  permissionIds: Array<number>
}

export interface Attachment {
  id: number
  originalFileName: string
  relativePath: string
  fileSizeInByte: number
  type: AttachmentType
}

export interface UserInfo {
  id: 1
  fullName: string
  motherName: string
  username: string
  nationalCode: string
  birthDate: string
  phoneNumber: string
  address: string
  creationStep: 1
  salaryDetail: SalaryDetailsFormTypes
  attachments: Array<Attachment>
  branches: Array<Warehouse>
  roles: Array<Role>
  allPermissions: Array<RolePermission>
}

export interface UserDailyReport {
  date: string
  totalInstallmentAmount: number
  totalDeliveredCashAmount: number
  cumulativeUndeliveredCashAmount: number
}
