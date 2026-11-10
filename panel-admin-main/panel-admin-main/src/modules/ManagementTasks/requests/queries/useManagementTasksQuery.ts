import { computed, type Ref } from 'vue'
import { keepPreviousData, useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { ManagementTaskEndpoints } from '../../domain/endpoints'
import type {
  ManagementTaskListFilters,
  ManagementTaskListItem,
  ManagementTaskPaginated,
} from '../../domain/types'

export const MANAGEMENT_TASKS_LIST_KEY = 'management-tasks-list'

export function fetchManagementTasks(
  pageIndex: number,
  pageSize: number,
  filters: ManagementTaskListFilters
) {
  return axiosInstance
    .get<ManagementTaskPaginated<ManagementTaskListItem>>(
      ManagementTaskEndpoints.list,
      {
        params: {
          pageIndex,
          pageSize,
          branchId: filters.branchId || undefined,
          status: filters.status ?? undefined,
          search: filters.search.trim() || undefined,
          inboxForMe: filters.inboxForMe ? true : undefined,
        },
      }
    )
    .then((r) => r.data)
}

export function useManagementTasksQuery(
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  filters: Ref<ManagementTaskListFilters>
) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      MANAGEMENT_TASKS_LIST_KEY,
      auth.selectedBranch?.id ?? null,
      auth.branchGeneration,
      filters.value.status,
      filters.value.search,
      filters.value.inboxForMe,
      pageIndex.value,
      pageSize.value,
    ]),
    enabled: computed(
      () => auth.permissionsReady && filters.value.branchId != null
    ),
    placeholderData: keepPreviousData,
    queryFn: () =>
      fetchManagementTasks(pageIndex.value, pageSize.value, filters.value),
  })
}
