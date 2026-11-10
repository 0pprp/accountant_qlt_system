import type { AuthLoginForm, PermissionTypes } from './model'
import type { Branch } from '@/modules/Branch/types/model'

export type AuthLoginPayload = AuthLoginForm

/**
 * عقد الاستجابة الفعلي لـ login/refresh (SignInResponse).
 * الحقول الأساسية: tokens + permissions + branches.
 * isSuperAdmin اختياري للتوافق مع الواجهة الحالية إن وُجد في الحمولة.
 * roles اختيارية كما في السيرفر — لا نختلق دلالة صلاحيات دور جديدة.
 */
export type AuthLoginSuccessResponse = {
  refreshToken: string
  accessToken: string
  permissions: PermissionTypes
  branches: Array<Branch>
  roles?: string[]
  isSuperAdmin?: boolean
}

export interface RefreshTokenPayload {
  refreshToken: string
}
