import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { FollowerWorkEndpoints } from '../../domain/endpoints'
import { FOLLOWER_WORK_KEY } from '../../domain/queryKeys'
import type { SetFollowerCollectionEnabledResult } from '../../domain/collectionTypes'

export function useSetFollowerCollectionEnabledMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (input: {
      listId: number
      enabled: boolean
      expectedVersion: number
    }) => {
      const { data } = await axiosInstance.patch<SetFollowerCollectionEnabledResult>(
        FollowerWorkEndpoints.followerCollection(input.listId),
        {
          enabled: input.enabled,
          expectedVersion: input.expectedVersion,
        }
      )
      return data
    },
    onSuccess: async () => {
      await qc.invalidateQueries({ queryKey: [FOLLOWER_WORK_KEY, 'collection'] })
    },
  })
}
