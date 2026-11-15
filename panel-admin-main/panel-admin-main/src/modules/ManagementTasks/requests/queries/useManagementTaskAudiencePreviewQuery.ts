import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import {
  ManagementTaskAudience,
  type ManagementTaskAudiencePreview,
  type ManagementTaskAudienceValue,
} from '../../domain/types'

export const MANAGEMENT_TASK_AUDIENCE_PREVIEW_KEY = 'management-task-audience-preview'

export function useManagementTaskAudiencePreviewQuery(
  branchId: Ref<number | null>,
  audience: Ref<ManagementTaskAudienceValue>,
  enabled: Ref<boolean>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      MANAGEMENT_TASK_AUDIENCE_PREVIEW_KEY,
      auth.branchGeneration,
      branchId.value,
      audience.value,
    ]),
    enabled: computed(
      () =>
        enabled.value &&
        branchId.value != null &&
        branchId.value > 0 &&
        audience.value !== ManagementTaskAudience.User
    ),
    staleTime: 30_000,
    queryFn: async () => {
      const { data } = await axiosInstance.get<ManagementTaskAudiencePreview>(
        ManagementTaskEndpoints.audiencePreview,
        {
          params: {
            branchId: branchId.value,
            audience: audience.value,
          },
        }
      )
      return data
    },
  })
}
