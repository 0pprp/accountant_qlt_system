import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { SalesLegalEndpoints } from '../../domain/endpoints'
import type { ListLegalFollowupsResponse } from '../../domain/types'

export const LEGAL_FOLLOWUPS_KEY = 'sales-legal-followups'

export function useLegalFollowupsQuery(params: {
  branchId: Ref<number | null>
  status: Ref<string>
  search: Ref<string>
  page: Ref<number>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      LEGAL_FOLLOWUPS_KEY,
      auth.user?.id ?? 'anon',
      params.branchId.value ?? 'all',
      params.status.value,
      params.search.value,
      params.page.value,
    ]),
    enabled: computed(() => enabledSafe(params.enabled)),
    queryFn: async () => {
      const q: Record<string, string | number> = {
        page: params.page.value,
        pageSize: 20,
        status: params.status.value,
      }
      if (params.branchId.value) q.branchId = params.branchId.value
      if (params.search.value.trim()) q.search = params.search.value.trim()
      const { data } = await axiosInstance.get<ListLegalFollowupsResponse>(
        SalesLegalEndpoints.list,
        { params: q }
      )
      return data
    },
  })
}

function enabledSafe(enabled: Ref<boolean>) {
  return enabled.value === true
}
