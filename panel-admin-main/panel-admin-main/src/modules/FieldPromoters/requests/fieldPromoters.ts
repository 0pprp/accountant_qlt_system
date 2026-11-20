import { computed, toValue, type MaybeRefOrGetter } from 'vue'
import { keepPreviousData, useMutation, useQuery, useQueryClient } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import { useAuthStore } from '@/modules/Auth/store'

export type PromoterStatus = 'working' | 'stationary' | 'notStarted' | 'ended' | 'suspended'
export type OrderStage = 'inProgress' | 'sold' | 'finished' | 'rejected'
export type AttendanceStatus = 'Met' | 'Missed' | 'Off' | 'NoTarget' | 'Today'

export interface PromoterLiveRow {
  id: number
  fullName: string
  phone: string | null
  status: PromoterStatus
  statusLabel: string
  lat: number | null
  lng: number | null
  lastAt: string | null
  shiftStartedAt: string | null
  workMinutes: number
  targetMinutes: number
  pins: number
  orders: number
  targetPercent: number
  isSuspended: boolean
}

export interface PromotersLive {
  total: number
  working: number
  stationary: number
  notStarted: number
  pinsToday: number
  interestedPinsToday: number
  ordersToday: number
  soldToday: number
  unseenAlerts: number
  metYesterday: number
  targetedYesterday: number
  promoters: PromoterLiveRow[]
}

export interface PromoterSession {
  startedAt: string
  endedAt: string | null
}

export interface PromoterPeriod {
  start: string
  end: string
  minutes: number
  events: number
}

export interface RoutePoint {
  lat: number
  lng: number
  at: string
}

export interface DayPin {
  id: number
  lat: number
  lng: number
  name: string
  interest: number
  interestLabel: string
  phone: string | null
  note: string | null
  at: string
  minutesSincePrevious: number | null
  quick: boolean
}

export interface DayOrder {
  id: number
  customerName: string
  product: string
  statusLabel: string
  at: string | null
}

export interface DayAlert {
  id: number
  lat: number
  lng: number
  since: string
  lastSeen: string
  resolvedAt: string | null
  minutes: number
}

export interface PromoterDay {
  userId: number
  fullName: string
  date: string
  firstShiftStart: string | null
  workMinutes: number
  targetMinutes: number
  targetStatus: string
  pins: number
  orders: number
  sessions: PromoterSession[]
  periods: PromoterPeriod[]
  route: RoutePoint[]
  pinList: DayPin[]
  orderList: DayOrder[]
  alerts: DayAlert[]
}

export interface AttendanceCell {
  day: string
  status: AttendanceStatus
  label: string
  workMinutes: number
  targetMinutes: number
  pins: number
  orders: number
}

export interface AttendanceRow {
  userId: number
  fullName: string
  targetMinutes: number
  met: number
  missed: number
  off: number
  days: AttendanceCell[]
}

export interface Attendance {
  from: string
  to: string
  days: string[]
  rows: AttendanceRow[]
}

export interface PromoterAlert {
  id: number
  userId: number
  fullName: string
  lat: number
  lng: number
  since: string
  lastSeen: string
  resolvedAt: string | null
  minutes: number
  seen: boolean
}

export interface PromoterAlertsPage {
  items: PromoterAlert[]
  unseen: number
  page: number
  hasMore: boolean
}

export interface AdminPin {
  id: number
  lat: number
  lng: number
  name: string
  interest: number
  interestLabel: string
  phone: string | null
  note: string | null
  userId: number
  promoterName: string
  createdAt: string
}

export interface AdminPinsPage {
  items: AdminPin[]
  total: number
  page: number
  hasMore: boolean
}

export interface SaleProgress {
  total: number
  paid: number
  remaining: number
  paymentsCount: number
  lastPaymentDate: string | null
  percent: number
}

export interface PromoterOrder {
  id: number
  customerName: string
  customerPhone: string
  product: string
  address: string
  note: string | null
  branchName: string
  stage: OrderStage
  statusLabel: string
  reason: string | null
  createdAt: string | null
  canCancel: boolean
  sale: SaleProgress | null
}

export interface AdminPromoterOrder {
  order: PromoterOrder
  userId: number
  promoterName: string
  hasShopPhoto: boolean
  shopLat: number | null
  shopLng: number | null
}

export interface OrderCounts {
  inProgress: number
  sold: number
  finished: number
  rejected: number
}

export interface AdminOrdersPage {
  items: AdminPromoterOrder[]
  total: number
  page: number
  hasMore: boolean
  counts: OrderCounts
}

export interface PromoterAccount {
  id: number
  fullName: string
  username: string
  phone: string | null
  branches: { id: number; name: string }[]
  isSuspended: boolean
  suspendReason: string | null
  targetMinutes: number | null
}

export interface PromoterSettings {
  defaultDailyWorkMinutes: number
  stationaryRadiusMeters: number
  stationaryMinutes: number
  supportPhone: string | null
  supportWhatsapp: string | null
  locationRetentionDays: number
}

export interface PinFilters {
  branchId: number | null
  userId: number | null
  minInterest: number | null
  from: string
  to: string
  q: string
  page: number
}

export interface OrderFilters {
  branchId: number | null
  userId: number | null
  stage: OrderStage | 'all'
  from: string
  to: string
  page: number
}

export const FP_KEY = 'field-promoters'
const BASE = '/admin/field-promoters'

function useCanRead() {
  const auth = useAuthStore()
  return computed(() => auth.permissionsReady && auth.hasPermission('FieldPromoter', 'Read') === true)
}

export function usePromoterPermissions() {
  const auth = useAuthStore()
  return {
    canRead: computed(() => auth.hasPermission('FieldPromoter', 'Read') === true),
    canManage: computed(() => auth.hasPermission('FieldPromoter', 'Manage') === true),
  }
}

function clean<T extends Record<string, unknown>>(params: T) {
  return Object.fromEntries(
    Object.entries(params).filter(([, v]) => v !== null && v !== undefined && v !== '')
  )
}

export function usePromotersLiveQuery(branchId: MaybeRefOrGetter<number | null>, poll = false) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'live', toValue(branchId)]),
    enabled: canRead,
    refetchInterval: poll ? 20_000 : false,
    refetchIntervalInBackground: false,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<PromotersLive>(`${BASE}/live`, {
        signal,
        params: clean({ branchId: toValue(branchId) }),
      })
      return data
    },
  })
}

export function usePromoterDayQuery(userId: MaybeRefOrGetter<number | null>, date: MaybeRefOrGetter<string>) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'day', toValue(userId), toValue(date)]),
    enabled: computed(() => canRead.value && toValue(userId) != null),
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<PromoterDay>(`${BASE}/${toValue(userId)}/day`, {
        signal,
        params: clean({ date: toValue(date) }),
      })
      return data
    },
  })
}

export function useAttendanceQuery(params: MaybeRefOrGetter<{ from: string; to: string; branchId: number | null }>, active: MaybeRefOrGetter<boolean> = true) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'attendance', toValue(params)]),
    enabled: computed(() => canRead.value && toValue(active)),
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<Attendance>(`${BASE}/attendance`, {
        signal,
        params: clean(toValue(params)),
      })
      return data
    },
  })
}

export async function downloadAttendanceExport(params: { from: string; to: string; branchId: number | null }) {
  const { data } = await axiosInstance.get<Blob>(`${BASE}/attendance/export`, {
    params: clean(params),
    responseType: 'blob',
  })
  const url = URL.createObjectURL(data)
  const a = document.createElement('a')
  a.href = url
  a.download = `promoters-attendance-${params.from}-${params.to}.xlsx`
  document.body.appendChild(a)
  a.click()
  a.remove()
  setTimeout(() => URL.revokeObjectURL(url), 1000)
}

export function useAlertsQuery(params: MaybeRefOrGetter<{ openOnly: boolean; page: number }>) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'alerts', toValue(params)]),
    enabled: canRead,
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const p = toValue(params)
      const { data } = await axiosInstance.get<PromoterAlertsPage>(`${BASE}/alerts`, {
        signal,
        params: { openOnly: p.openOnly, page: p.page },
      })
      return data
    },
  })
}

export function useMarkAlertSeenMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number | null) =>
      axiosInstance.post(id == null ? `${BASE}/alerts/seen-all` : `${BASE}/alerts/${id}/seen`).then((r) => r.data),
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: [FP_KEY, 'alerts'] })
      void qc.invalidateQueries({ queryKey: [FP_KEY, 'live'] })
    },
  })
}

export function usePinsQuery(filters: MaybeRefOrGetter<PinFilters>) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'pins', toValue(filters)]),
    enabled: canRead,
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<AdminPinsPage>(`${BASE}/pins`, {
        signal,
        params: clean({ ...toValue(filters), q: toValue(filters).q.trim() }),
      })
      return data
    },
  })
}

export function useDeletePinMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: number) => axiosInstance.delete(`${BASE}/pins/${id}`).then((r) => r.data),
    onSettled: () => {
      void qc.invalidateQueries({ queryKey: [FP_KEY, 'pins'] })
      void qc.invalidateQueries({ queryKey: [FP_KEY, 'live'] })
      void qc.invalidateQueries({ queryKey: [FP_KEY, 'day'] })
    },
  })
}

export function useOrdersQuery(filters: MaybeRefOrGetter<OrderFilters>, staleTime = 0) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'orders', toValue(filters)]),
    enabled: canRead,
    staleTime,
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const f = toValue(filters)
      const { data } = await axiosInstance.get<AdminOrdersPage>(`${BASE}/orders`, {
        signal,
        params: clean({ ...f, stage: f.stage === 'all' ? null : f.stage }),
      })
      return data
    },
  })
}

export async function fetchShopPhoto(orderId: number): Promise<string> {
  const { data } = await axiosInstance.get<Blob>(`${BASE}/orders/${orderId}/shop-photo`, { responseType: 'blob' })
  return URL.createObjectURL(data)
}

export function usePromoterAccountsQuery(params: MaybeRefOrGetter<{ q: string; status: '' | 'active' | 'suspended'; branchId: number | null }>) {
  const canRead = useCanRead()
  return useQuery({
    queryKey: computed(() => [FP_KEY, 'promoters', toValue(params)]),
    enabled: canRead,
    placeholderData: keepPreviousData,
    queryFn: async ({ signal }) => {
      const p = toValue(params)
      const { data } = await axiosInstance.get<PromoterAccount[]>(`${BASE}/promoters`, {
        signal,
        params: clean({ q: p.q.trim(), status: p.status, branchId: p.branchId }),
      })
      return data
    },
  })
}

function invalidateAll(qc: ReturnType<typeof useQueryClient>) {
  void qc.invalidateQueries({ queryKey: [FP_KEY] })
}

export function useSavePromoterMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, body }: { id: number | null; body: { fullName: string; username: string; phone: string | null; branchIds: number[]; password?: string } }) =>
      (id == null
        ? axiosInstance.post<number>(`${BASE}/promoters`, body)
        : axiosInstance.put<number>(`${BASE}/promoters/${id}`, body)
      ).then((r) => r.data),
    onSettled: () => invalidateAll(qc),
  })
}

export function useSetPromoterPasswordMutation() {
  return useMutation({
    mutationFn: ({ id, password }: { id: number; password: string }) =>
      axiosInstance.post(`${BASE}/promoters/${id}/password`, { password }).then((r) => r.data),
  })
}

export function useSuspendPromoterMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, suspended, reason }: { id: number; suspended: boolean; reason: string | null }) =>
      axiosInstance.post(`${BASE}/promoters/${id}/suspend`, { suspended, reason }).then((r) => r.data),
    onSettled: () => invalidateAll(qc),
  })
}

export function useSetPromoterTargetMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, minutes }: { id: number; minutes: number | null }) =>
      axiosInstance.put(`${BASE}/promoters/${id}/target`, { minutes }).then((r) => r.data),
    onSettled: () => invalidateAll(qc),
  })
}

export function useSettingsQuery() {
  const canRead = useCanRead()
  return useQuery({
    queryKey: [FP_KEY, 'settings'],
    enabled: canRead,
    queryFn: async ({ signal }) => {
      const { data } = await axiosInstance.get<PromoterSettings>(`${BASE}/settings`, { signal })
      return data
    },
  })
}

export function useSaveSettingsMutation() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: PromoterSettings) => axiosInstance.put<PromoterSettings>(`${BASE}/settings`, body).then((r) => r.data),
    onSuccess: (data) => qc.setQueryData([FP_KEY, 'settings'], data),
    onSettled: () => invalidateAll(qc),
  })
}

export function apiErrorMessage(e: unknown, fallback = 'تعذر تنفيذ الإجراء'): string {
  const ax = e as { response?: { status?: number; data?: { detail?: string; errors?: { message?: string }[] | Record<string, string[]> } } }
  const errors = ax.response?.data?.errors
  if (Array.isArray(errors) && errors[0]?.message) return errors[0].message
  if (errors && !Array.isArray(errors)) {
    const first = Object.values(errors)[0]
    if (Array.isArray(first) && first[0]) return first[0]
  }
  if (ax.response?.data?.detail) return ax.response.data.detail
  if (ax.response?.status === 403) return 'لا تملك صلاحية تنفيذ هذا الإجراء'
  if (ax.response?.status === 404) return 'العنصر غير موجود'
  return fallback
}
