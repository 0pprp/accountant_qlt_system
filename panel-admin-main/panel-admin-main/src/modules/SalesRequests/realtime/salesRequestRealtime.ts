import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/vue-query'
import { computed, onUnmounted, ref, watch, type Ref } from 'vue'
import useAuthStore from '@/modules/Auth/store'
import { SALES_REQUESTS_QUERY_KEY } from '../requests/queries/useSalesRequestsQuery'
import { SALES_REQUEST_COUNTS_KEY } from '../requests/queries/useSalesRequestCountsQuery'
import { SALES_REQUEST_BY_ID_KEY } from '../requests/queries/useSalesRequestByIdQuery'
import { SALES_REQUEST_TIMELINE_KEY } from '../requests/queries/useSalesRequestTimelineQuery'
import { UNASSIGNED_QUERY_KEY } from '../requests/queries/useUnassignedSalesRequestsQuery'
import { hubOriginLabel, resolveSalesRequestHubUrl } from './hubUrl'

export type RealtimeConnectionState = 'connected' | 'reconnecting' | 'disconnected'

export interface SalesRequestChangedEvent {
  eventId: string
  requestId: number
  revision: number
  occurredAtUtc: string
  changeType: string
  branchId: number
  targetEmployeeId?: number | null
  previousTargetEmployeeId?: number | null
  audience?: string
  EventId?: string
  RequestId?: number
  Revision?: number
  OccurredAtUtc?: string
  ChangeType?: string
  BranchId?: number
  TargetEmployeeId?: number | null
  PreviousTargetEmployeeId?: number | null
  Audience?: string
}

function sanitizeConnectError(err: unknown): string {
  const raw = err instanceof Error ? err.message : String(err ?? 'unknown')
  // لا تُسرّب query string التي قد تحمل access_token
  return raw
    .replace(/access_token=[^&\s)]+/gi, 'access_token=***')
    .replace(/Bearer\s+[A-Za-z0-9._-]+/gi, 'Bearer ***')
    .slice(0, 240)
}

/**
 * عميل SignalR للوحة — متصل فقط عند HubConnectionState.Connected الفعلي.
 * polling معلن كتعافٍ عند الانقطاع. لا withCredentials مع AllowAnyOrigin.
 */
export function useSalesRequestRealtime(options: {
  enabled: Ref<boolean>
  openRequestId: Ref<number | null>
  onOwnershipLost?: (requestId: number) => void
}) {
  const auth = useAuthStore()
  const qc = useQueryClient()

  const connectionState = ref<RealtimeConnectionState>('disconnected')
  const lastSyncedAt = ref<number | null>(null)
  const lastFailure = ref<string | null>(null)
  const hubOrigin = ref<string>('')
  const seenEvents = new Set<string>()
  const latestRevision = new Map<number, number>()
  let connection: HubConnection | null = null
  let invalidateTimer: ReturnType<typeof setTimeout> | null = null
  let pendingIds = new Set<number>()
  let sessionGenAttached: number | null = null
  let pollTimer: ReturnType<typeof setInterval> | null = null
  let connectSeq = 0
  let connecting = false

  const statusLabel = computed(() => {
    // لا تُظهر «متصل» إلا إذا الحالة الفعلية Connected
    if (
      connectionState.value === 'connected' &&
      connection?.state === HubConnectionState.Connected
    ) {
      return 'متصل'
    }
    if (connectionState.value === 'reconnecting') return 'يعاد الاتصال'
    return 'التحديث اللحظي غير متصل — تحديث دوري'
  })

  function scheduleInvalidate(requestId: number) {
    pendingIds.add(requestId)
    if (invalidateTimer) return
    invalidateTimer = setTimeout(() => {
      invalidateTimer = null
      const ids = [...pendingIds]
      pendingIds = new Set()
      void qc.invalidateQueries({ queryKey: [SALES_REQUESTS_QUERY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUEST_COUNTS_KEY] })
      void qc.invalidateQueries({ queryKey: [UNASSIGNED_QUERY_KEY] })
      for (const id of ids) {
        if (id > 0) {
          void qc.invalidateQueries({ queryKey: [SALES_REQUEST_BY_ID_KEY, id] })
          void qc.invalidateQueries({ queryKey: [SALES_REQUEST_TIMELINE_KEY, id] })
        }
      }
      lastSyncedAt.value = Date.now()
    }, 180)
  }

  function handleEvent(raw: SalesRequestChangedEvent) {
    const eventId = String(raw.eventId || raw.EventId || '')
    const requestId = Number(raw.requestId ?? raw.RequestId)
    const revision = Number(raw.revision ?? raw.Revision ?? 0)
    if (!eventId || !requestId) return
    if (seenEvents.has(eventId)) return
    seenEvents.add(eventId)
    if (seenEvents.size > 500) {
      const first = seenEvents.values().next().value
      if (first) seenEvents.delete(first)
    }
    const prev = latestRevision.get(requestId) ?? -1
    if (revision < prev) return
    latestRevision.set(requestId, revision)

    scheduleInvalidate(requestId)

    const open = options.openRequestId.value
    if (open === requestId) {
      const change = String(raw.changeType || raw.ChangeType || '')
      if (
        change === 'Reassigned' ||
        change === 'Invalidation' ||
        change === 'Assigned'
      ) {
        options.onOwnershipLost?.(requestId)
      }
    }
  }

  function readToken(): string {
    // قراءة حيّة من المتجر عند كل negotiate/reconnect — يدعم التجديد
    return useAuthStore().accessToken || ''
  }

  async function stopConnection(opts?: { keepPoll?: boolean }) {
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
    if (!opts?.keepPoll) {
      // يقرر المستدعي إن كان سيبدأ polling
    }
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
      await stopConnection({ keepPoll: true })
      if (seq !== connectSeq) return

      sessionGenAttached = useAuthStore().sessionGeneration

      let url: string
      try {
        url = resolveSalesRequestHubUrl()
        hubOrigin.value = hubOriginLabel(url)
      } catch (e) {
        lastFailure.value = sanitizeConnectError(e)
        connectionState.value = 'disconnected'
        startFallbackPoll()
        return
      }

      const hub = new HubConnectionBuilder()
        .withUrl(url, {
          // توكن عبر access_token في query (WebSockets/SSE/LP) — بدون withCredentials
          accessTokenFactory: () => readToken(),
          withCredentials: false,
        })
        .withAutomaticReconnect([0, 1000, 2000, 5000, 10000])
        .configureLogging(LogLevel.Warning)
        .build()

      hub.on('SalesRequestChanged', (payload: SalesRequestChangedEvent) => {
        handleEvent(payload)
      })

      hub.onreconnecting(() => {
        if (seq !== connectSeq) return
        connectionState.value = 'reconnecting'
        startFallbackPoll()
      })
      hub.onreconnected(async () => {
        if (seq !== connectSeq) return
        if (hub.state !== HubConnectionState.Connected) {
          connectionState.value = 'disconnected'
          startFallbackPoll()
          return
        }
        connectionState.value = 'connected'
        lastFailure.value = null
        stopFallbackPoll()
        try {
          await hub.invoke('RefreshSubscriptions')
        } catch {
          /* ignore */
        }
        scheduleInvalidate(options.openRequestId.value ?? 0)
        void qc.invalidateQueries({ queryKey: [SALES_REQUESTS_QUERY_KEY] })
        void qc.invalidateQueries({ queryKey: [SALES_REQUEST_COUNTS_KEY] })
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
      lastSyncedAt.value = Date.now()
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
    sessionGenAttached = null
  }

  function startFallbackPoll() {
    if (pollTimer) return
    if (typeof document !== 'undefined' && document.visibilityState === 'hidden')
      return
    pollTimer = setInterval(() => {
      if (typeof document !== 'undefined' && document.visibilityState === 'hidden')
        return
      if (!options.enabled.value) return
      // لا تُلغِ حالة «متصل» — هذا مسار تعافٍ معلن فقط عند الانقطاع
      if (connection?.state === HubConnectionState.Connected) return
      void qc.invalidateQueries({ queryKey: [SALES_REQUESTS_QUERY_KEY] })
      void qc.invalidateQueries({ queryKey: [SALES_REQUEST_COUNTS_KEY] })
      const open = options.openRequestId.value
      if (open) {
        void qc.invalidateQueries({ queryKey: [SALES_REQUEST_BY_ID_KEY, open] })
      }
      lastSyncedAt.value = Date.now()
    }, 8000)
  }

  function stopFallbackPoll() {
    if (pollTimer) {
      clearInterval(pollTimer)
      pollTimer = null
    }
  }

  watch(
    () =>
      [
        options.enabled.value,
        auth.isAuthenticated,
        auth.accessToken,
        auth.sessionGeneration,
      ] as const,
    async ([enabled, isAuth, token, gen]) => {
      if (!enabled || !isAuth || !token) {
        await disconnect()
        return
      }
      if (sessionGenAttached != null && sessionGenAttached !== gen) {
        // تبديل حساب / إنهاء جلسة: اقطع قبل إعادة الربط بتوكن الجيل الجديد
        await disconnect()
      }
      await connect()
    },
    { immediate: true }
  )

  if (typeof document !== 'undefined') {
    document.addEventListener('visibilitychange', () => {
      if (document.visibilityState === 'hidden') stopFallbackPoll()
      else if (
        connectionState.value !== 'connected' ||
        connection?.state !== HubConnectionState.Connected
      ) {
        startFallbackPoll()
        void connect()
      }
    })
  }

  onUnmounted(() => {
    void disconnect()
  })

  return {
    connectionState,
    statusLabel,
    lastSyncedAt,
    lastFailure,
    hubOrigin,
    connect,
    disconnect,
  }
}
