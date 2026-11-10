import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/vue-query'
import { computed, onUnmounted, ref, watch, type Ref } from 'vue'
import useAuthStore from '@/modules/Auth/store'
import { MANAGEMENT_TASKS_LIST_KEY } from '../requests/queries/useManagementTasksQuery'
import { MANAGEMENT_TASK_DETAIL_KEY } from '../requests/queries/useManagementTaskDetailQuery'
import { resolveManagementTaskHubUrl } from './hubUrl'

export type RealtimeConnectionState = 'connected' | 'reconnecting' | 'disconnected'

export interface ManagementTaskChangedEvent {
  taskId?: number
  TaskId?: number
  revision?: number
  Revision?: number
  changeType?: number | string
  ChangeType?: number | string
  branchId?: number
  BranchId?: number
  holderId?: number
  HolderId?: number
  assigneeId?: number
  AssigneeId?: number
  eventId?: string
  EventId?: string
}

function sanitizeConnectError(err: unknown): string {
  const raw = err instanceof Error ? err.message : String(err ?? 'unknown')
  return raw
    .replace(/access_token=[^&\s)]+/gi, 'access_token=***')
    .replace(/Bearer\s+[A-Za-z0-9._-]+/gi, 'Bearer ***')
    .slice(0, 240)
}

/**
 * عميل SignalR لمهام الإدارة — «متصل» فقط عند Connected الفعلي.
 * polling معلن عند الانقطاع. لا withCredentials مع AllowAnyOrigin.
 */
export function useManagementTaskRealtime(options: {
  enabled: Ref<boolean>
  openTaskId: Ref<number | null>
}) {
  const auth = useAuthStore()
  const qc = useQueryClient()

  const connectionState = ref<RealtimeConnectionState>('disconnected')
  const lastFailure = ref<string | null>(null)
  const seenEvents = new Set<string>()
  const latestRevision = new Map<number, number>()
  let connection: HubConnection | null = null
  let invalidateTimer: ReturnType<typeof setTimeout> | null = null
  let pendingIds = new Set<number>()
  let pollTimer: ReturnType<typeof setInterval> | null = null
  let connectSeq = 0
  let connecting = false

  const statusLabel = computed(() => {
    if (
      connectionState.value === 'connected' &&
      connection?.state === HubConnectionState.Connected
    ) {
      return 'متصل'
    }
    if (connectionState.value === 'reconnecting') return 'يعاد الاتصال'
    return 'التحديث اللحظي غير متصل — تحديث دوري'
  })

  function scheduleInvalidate(taskId: number) {
    pendingIds.add(taskId)
    if (invalidateTimer) return
    invalidateTimer = setTimeout(() => {
      invalidateTimer = null
      const ids = [...pendingIds]
      pendingIds = new Set()
      void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASKS_LIST_KEY] })
      for (const id of ids) {
        if (id > 0) {
          void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASK_DETAIL_KEY] })
        }
      }
    }, 180)
  }

  function handleEvent(raw: ManagementTaskChangedEvent) {
    const taskId = Number(raw.taskId ?? raw.TaskId ?? 0)
    const revision = Number(raw.revision ?? raw.Revision ?? 0)
    const eventId = String(
      raw.eventId ||
        raw.EventId ||
        `${taskId}:${revision}:${raw.changeType ?? raw.ChangeType ?? ''}`
    )
    if (!taskId || !eventId) return
    if (seenEvents.has(eventId)) return
    seenEvents.add(eventId)
    if (seenEvents.size > 500) {
      const first = seenEvents.values().next().value
      if (first) seenEvents.delete(first)
    }
    const prev = latestRevision.get(taskId) ?? -1
    if (revision < prev) return
    latestRevision.set(taskId, revision)
    scheduleInvalidate(taskId)
  }

  function readToken(): string {
    return useAuthStore().accessToken || ''
  }

  function stopFallbackPoll() {
    if (pollTimer) {
      clearInterval(pollTimer)
      pollTimer = null
    }
  }

  function startFallbackPoll() {
    if (pollTimer || !options.enabled.value) return
    pollTimer = setInterval(() => {
      void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASKS_LIST_KEY] })
      const open = options.openTaskId.value
      if (open) {
        void qc.invalidateQueries({ queryKey: [MANAGEMENT_TASK_DETAIL_KEY] })
      }
    }, 8000)
  }

  async function stopConnection() {
    const current = connection
    connection = null
    if (current) {
      current.onclose(() => undefined)
      try {
        await current.stop()
      } catch {
        /* ignore */
      }
    }
    connectionState.value = 'disconnected'
  }

  async function connect() {
    if (!options.enabled.value) return
    const token = readToken()
    if (!token) return
    if (connecting) return
    if (connection?.state === HubConnectionState.Connected) {
      connectionState.value = 'connected'
      return
    }

    const seq = ++connectSeq
    connecting = true
    stopFallbackPoll()

    try {
      await stopConnection()
      if (seq !== connectSeq) return

      let url: string
      try {
        url = resolveManagementTaskHubUrl()
      } catch (e) {
        lastFailure.value = sanitizeConnectError(e)
        connectionState.value = 'disconnected'
        startFallbackPoll()
        return
      }

      const hub = new HubConnectionBuilder()
        .withUrl(url, {
          accessTokenFactory: () => readToken(),
          withCredentials: false,
        })
        .withAutomaticReconnect([0, 1000, 2000, 5000, 10000])
        .configureLogging(LogLevel.Warning)
        .build()

      hub.on('managementTaskChanged', (payload: ManagementTaskChangedEvent) => {
        handleEvent(payload)
      })

      hub.onreconnecting(() => {
        if (seq !== connectSeq) return
        connectionState.value = 'reconnecting'
        startFallbackPoll()
      })
      hub.onreconnected(() => {
        if (seq !== connectSeq) return
        if (hub.state !== HubConnectionState.Connected) {
          connectionState.value = 'disconnected'
          startFallbackPoll()
          return
        }
        connectionState.value = 'connected'
        lastFailure.value = null
        stopFallbackPoll()
        scheduleInvalidate(options.openTaskId.value ?? 0)
      })
      hub.onclose(() => {
        if (connection !== hub && connection != null) return
        connectionState.value = 'disconnected'
        startFallbackPoll()
      })

      connection = hub
      await hub.start()

      if (seq !== connectSeq) {
        try {
          await hub.stop()
        } catch {
          /* ignore */
        }
        return
      }

      if (hub.state !== HubConnectionState.Connected) {
        lastFailure.value = 'hub_start_not_connected'
        connectionState.value = 'disconnected'
        startFallbackPoll()
        return
      }

      connectionState.value = 'connected'
      lastFailure.value = null
      stopFallbackPoll()
    } catch (err) {
      if (seq !== connectSeq) return
      lastFailure.value = sanitizeConnectError(err)
      connectionState.value = 'disconnected'
      connection = null
      startFallbackPoll()
    } finally {
      if (seq === connectSeq) connecting = false
    }
  }

  async function disconnect() {
    connectSeq++
    connecting = false
    stopFallbackPoll()
    if (invalidateTimer) {
      clearTimeout(invalidateTimer)
      invalidateTimer = null
    }
    await stopConnection()
  }

  watch(
    () =>
      [
        options.enabled.value,
        auth.accessToken,
        auth.sessionGeneration,
        auth.selectedBranch?.id ?? null,
        auth.branchGeneration,
      ] as const,
    async ([enabled], _, onCleanup) => {
      onCleanup(() => {
        /* watch handles disconnect via enabled=false */
      })
      if (!enabled || !auth.accessToken) {
        await disconnect()
        return
      }
      await connect()
    },
    { immediate: true }
  )

  onUnmounted(() => {
    void disconnect()
  })

  return {
    connectionState,
    statusLabel,
    lastFailure,
  }
}
