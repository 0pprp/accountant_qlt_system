import { type UseMutationOptions, useMutation } from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { SaleCreatePayload } from '@/modules/Sale/types/api'
import { createFormData } from '@/modules/Core/utils/formDataGenerator'

const ENDPOINT = '/admin/Orders'

export function createSale(payload: SaleCreatePayload): Promise<AxiosResponse> {
  const formData = createFormData(payload)

  return axiosInstance.post(ENDPOINT, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
}

export function useCreateSaleMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    SaleCreatePayload,
    unknown
  >
) {
  return useMutation({
    mutationFn: createSale,
    ...options,
  })
}
