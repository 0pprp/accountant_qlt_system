import type { AuthLoginForm, PermissionTypes } from './model'
import type { Branch } from '@/modules/Branch/types/model'

export type AuthLoginPayload = AuthLoginForm
export type AuthLoginSuccessResponse = {
  refreshToken: string
  accessToken: string
  permissions: PermissionTypes
  branches: Array<Branch>
  isSuperAdmin: boolean
}

export interface RefreshTokenPayload {
  refreshToken: string
}
