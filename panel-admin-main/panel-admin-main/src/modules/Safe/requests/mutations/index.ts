import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type {
  CreateSafePayload,
  CreateSafeResponse,
  SafeAmountPayload,
  SafeTransferPayload,
} from '../../types/api'
import { QUERY_KEY_SAFES } from '../queries/useSafesQuery'
import { QUERY_KEY_SAFE_MOVEMENTS } from '../queries/useSafeMovementsQuery'
import { QUERY_KEY_SAFE_VERIFY } from '../queries/useVerifySafeQuery'

/** كل عملية تُبطل القوائم والسجل والتحقق معاً حتى لا يبقى رصيد قديم على الشاشة. */
function invalidateSafeData(queryClient: ReturnType<typeof useQueryClient>) {
  void queryClient.invalidateQueries({ queryKey: [QUERY_KEY_SAFES] })
  void queryClient.invalidateQueries({ queryKey: [QUERY_KEY_SAFE_MOVEMENTS] })
  void queryClient.invalidateQueries({ queryKey: [QUERY_KEY_SAFE_VERIFY] })
}

export function useSafeDepositMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ safeId, payload }: { safeId: number; payload: SafeAmountPayload }) => {
      const { data } = await axiosInstance.post(`/admin/Safes/${safeId}/deposit`, payload)
      return data
    },
    onSuccess: () => invalidateSafeData(queryClient),
    meta: {
      success: 'safe.operations.depositSuccess',
    },
  })
}

export function useSafeWithdrawMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async ({ safeId, payload }: { safeId: number; payload: SafeAmountPayload }) => {
      const { data } = await axiosInstance.post(`/admin/Safes/${safeId}/withdraw`, payload)
      return data
    },
    onSuccess: () => invalidateSafeData(queryClient),
    meta: {
      success: 'safe.operations.withdrawSuccess',
    },
  })
}

export function useSafeTransferMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: SafeTransferPayload) => {
      const { data } = await axiosInstance.post('/admin/Safes/transfer', payload)
      return data
    },
    onSuccess: () => invalidateSafeData(queryClient),
    meta: {
      success: 'safe.operations.transferSuccess',
    },
  })
}

export function useCreateSafeMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (payload: CreateSafePayload) => {
      const { data } = await axiosInstance.post<CreateSafeResponse>('/admin/Safes', payload)
      return data
    },
    onSuccess: () => invalidateSafeData(queryClient),
    meta: {
      success: 'safe.operations.createSuccess',
    },
  })
}

export function useDeleteSafeMutation() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: async (safeId: number) => {
      const { data } = await axiosInstance.delete(`/admin/Safes/${safeId}`)
      return data
    },
    onSuccess: () => invalidateSafeData(queryClient),
    meta: {
      success: 'safe.operations.deleteSuccess',
    },
  })
}
