import {
  type UseMutationOptions,
  useMutation,
  useQueryClient,
} from '@tanstack/vue-query'
import type { AxiosResponse } from 'axios'
import { QUERY_KEY_PURCHASES_LIST } from '../queries'
import type { PurchasesCreatePayload } from '../../types/api'
import { axiosInstance } from '@/modules/Core/plugins/axios'

const ENDPOINT = '/admin/Purchases'

export function createPurchases(
  payload: PurchasesCreatePayload
): Promise<AxiosResponse> {
  const formData = new FormData()
  formData.append('factorNumber', payload.factorNumber!.toString())
  formData.append('safeType', payload.safeType!.toString())
  formData.append('branchId', payload.branchId?.toString() ?? '0')
  if (payload.attachments) {
    formData.append('Attachments', payload.attachments)
  }
  payload.purchaseItems.forEach((item, index) => {
    formData.append(
      `purchaseItems[${index}].productId`,
      item.productId!.toString()
    )
    formData.append(
      `purchaseItems[${index}].quantity`,
      item.quantity.toString()
    )
    formData.append(`purchaseItems[${index}].amount`, item.amount.toString())
  })

  return axiosInstance.post(ENDPOINT, formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
}

export function useCreatePurchasesMutation(
  options?: UseMutationOptions<
    AxiosResponse,
    AxiosCustomError,
    PurchasesCreatePayload,
    unknown
  >
) {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: createPurchases,
    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: [QUERY_KEY_PURCHASES_LIST],
      })
    },
    meta: {
      success: {
        title: 'successUpdate',
        description: 'successUpdateDescription',
      },
    },
    ...options,
  })
}
