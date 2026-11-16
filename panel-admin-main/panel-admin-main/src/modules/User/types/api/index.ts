import type {
  RolePermissionsResponse,
  SalaryDetailsFormTypes,
  PermissionsResponse,
  User,
  UserInfo,
  UserPermissionForm,
  UserDailyReport,
} from '../model'

export interface AvailableColumn {
  key: string
  displayName: string
  dataType: string
}

export type AvailableColumnsResponse = Array<AvailableColumn>

export type UserListServerSuccessResponse = {
  paginatedUsers: ServerSuccessPaginatedResponse<Array<User>>
}

/** حمولة إنشاء الحساب المبسّط — لا تفرض حقول الملف الوظيفي للتعديل. */
export type UserCreatePayload = {
  username: string
  password: string
  fullName: string
  roleId: number
  branchIds: number[]
  motherName?: string | null
  nationalCode?: string | null
  birthDate?: string | null
  address?: string | null
  phoneNumber?: string | null
}

export type SalaryDetailsPayload = SalaryDetailsFormTypes

export type RolePermissionListServerSuccessResponse = RolePermissionsResponse

export type PermissionListServerSuccessResponse = PermissionsResponse

export type UserPermissionPayload = UserPermissionForm

export type UserByIdServerSuccessResponse = UserInfo

export type UserDailyReportServerSuccessResponse = {
  items: Array<UserDailyReport>
  undeliveredCashAmount: number
}
