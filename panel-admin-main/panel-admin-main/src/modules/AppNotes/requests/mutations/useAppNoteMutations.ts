import { useMutation, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { AppNotesEndpoints } from '../../domain/endpoints'
import { APP_NOTES_LIST_KEY, APP_NOTES_UNREAD_KEY } from '../../domain/queryKeys'
import type { AppNoteDetail } from '../../domain/types'

async function invalidate(qc: ReturnType<typeof useQueryClient>) {
  await Promise.all([
    qc.invalidateQueries({ queryKey: [...APP_NOTES_LIST_KEY] }),
    qc.invalidateQueries({ queryKey: [...APP_NOTES_UNREAD_KEY] }),
  ])
}

export function useMarkAppNoteReadMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async (id: number) => {
      const { data } = await axiosInstance.put<AppNoteDetail>(
        AppNotesEndpoints.markRead(id)
      )
      return data
    },
    onSuccess: async () => invalidate(qc),
  })
}

export function useMarkAllAppNotesReadMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: async () => {
      const { data } = await axiosInstance.put<number>(
        AppNotesEndpoints.markAllRead
      )
      return data
    },
    onSuccess: async () => invalidate(qc),
  })
}
