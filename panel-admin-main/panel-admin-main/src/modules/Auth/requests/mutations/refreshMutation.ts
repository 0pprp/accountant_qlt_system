import type {
  AuthLoginSuccessResponse,
  RefreshTokenPayload,
} from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

export const ENDPOINT = `admin/Auth/refresh-token`

export async function refreshMutation(payload: RefreshTokenPayload) {
  return axiosInstance.post<AuthLoginSuccessResponse>(ENDPOINT, payload)
}
