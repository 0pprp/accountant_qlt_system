import axios from 'axios'
import type { AuthLoginSuccessResponse } from '@/modules/Auth/types/api'

const REFRESH_PATH = 'admin/Auth/refresh-token'

/**
 * طلب refresh خارج interceptor لتجنب الدوران المتبادل والاعتماد الدائري.
 */
export async function postRefreshToken(
  refreshToken: string
): Promise<AuthLoginSuccessResponse> {
  const baseURL = import.meta.env.VITE_API_BASE_URL
  const { data } = await axios.post<AuthLoginSuccessResponse>(
    `${baseURL}/${REFRESH_PATH}`.replace(/([^:]\/)\/+/g, '$1'),
    { refreshToken },
    {
      headers: { 'Content-Type': 'application/json' },
    }
  )
  return data
}
