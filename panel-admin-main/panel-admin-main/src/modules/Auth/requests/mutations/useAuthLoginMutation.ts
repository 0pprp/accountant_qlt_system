import { type UseMutationOptions, useMutation } from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type {
  AuthLoginPayload,
  AuthLoginSuccessResponse,
} from '@/modules/Auth/types/api'

const ENDPOINT = 'admin/Auth/login'

export function userLogin(
  payload: AuthLoginPayload
): Promise<AxiosResponse<AuthLoginSuccessResponse>> {
  return axiosInstance.post<AuthLoginSuccessResponse>(ENDPOINT, payload)
}

export function useAuthLoginMutation(
  options?: UseMutationOptions<
    AxiosResponse<AuthLoginSuccessResponse>,
    AxiosCustomError,
    AuthLoginPayload,
    unknown
  >
) {
  return useMutation({
    mutationFn: userLogin,
    ...options,
  })
}
