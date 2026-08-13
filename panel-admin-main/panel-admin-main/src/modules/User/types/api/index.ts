import type {
  RolePermissionsResponse,
  SalaryDetailsFormTypes,
  PermissionsResponse,
  User,
  UserForm,
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

export type UserCreatePayload = UserForm

export type SalaryDetailsPayload = SalaryDetailsFormTypes

export type RolePermissionListServerSuccessResponse = RolePermissionsResponse

export type PermissionListServerSuccessResponse = PermissionsResponse

export type UserPermissionPayload = UserPermissionForm

export type UserByIdServerSuccessResponse = UserInfo

export type UserDailyReportServerSuccessResponse = {
  items: Array<UserDailyReport>
  undeliveredCashAmount: number
}
