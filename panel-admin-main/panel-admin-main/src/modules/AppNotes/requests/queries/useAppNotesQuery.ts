import { computed, type Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'
import { AppNotesEndpoints } from '../../domain/endpoints'
import { APP_NOTES_LIST_KEY, APP_NOTES_UNREAD_KEY } from '../../domain/queryKeys'
import type { AppNoteDetail, AppNoteListResponse } from '../../domain/types'

export function useAppNotesQuery(params: {
  page: Ref<number>
  pageSize: Ref<number>
  unreadOnly: Ref<boolean>
  sourceApp: Ref<string>
  search: Ref<string>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      ...APP_NOTES_LIST_KEY,
      params.page.value,
      params.pageSize.value,
      params.unreadOnly.value,
      params.sourceApp.value,
      params.search.value,
      auth.branchGeneration,
      auth.sessionGeneration,
    ]),
    enabled: computed(
      () => params.enabled.value === true && auth.permissionsReady === true
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<AppNoteListResponse>(
        AppNotesEndpoints.list,
        {
          signal,
          params: {
            page: params.page.value,
            pageSize: params.pageSize.value,
            unreadOnly: params.unreadOnly.value || undefined,
            sourceApp: params.sourceApp.value || undefined,
            search: params.search.value || undefined,
          },
        }
      )
      return data
    },
  })
}

export function useAppNoteDetailQuery(params: {
  id: Ref<number | null>
  enabled: Ref<boolean>
}) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      'app-notes',
      'detail',
      params.id.value,
      auth.sessionGeneration,
    ]),
    enabled: computed(
      () =>
        params.enabled.value === true &&
        auth.permissionsReady === true &&
        params.id.value != null
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<AppNoteDetail>(
        AppNotesEndpoints.byId(params.id.value!),
        { signal }
      )
      return data
    },
  })
}

export function useAppNoteUnreadCountQuery(enabled: Ref<boolean>) {
  const auth = useAuthStore()
  return useQuery({
    queryKey: computed(() => [
      ...APP_NOTES_UNREAD_KEY,
      auth.sessionGeneration,
    ]),
    enabled: computed(
      () => enabled.value === true && auth.permissionsReady === true
    ),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<number>(
        AppNotesEndpoints.unreadCount,
        { signal }
      )
      return data
    },
  })
}
