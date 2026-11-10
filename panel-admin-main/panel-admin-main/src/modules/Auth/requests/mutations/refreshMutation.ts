import type {
  AuthLoginSuccessResponse,
  RefreshTokenPayload,
} from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export const ENDPOINT = `admin/Auth/refresh-token`
export const REFRESH_TOKEN_ENDPOINT = ENDPOINT

export async function refreshMutation(payload: RefreshTokenPayload) {
  const authStore = useAuthStore()
  return axiosInstance.post<AuthLoginSuccessResponse>(ENDPOINT, payload, {
    skipAuthRefresh: true,
    sessionGeneration: authStore.sessionGeneration,
  } as never)
}
