import { axiosInstance } from '@/modules/Core/plugins/axios'
import type {
  ArchiveActionResult,
  ArchiveBundleDetail,
  ArchiveBundleListItem,
  ArchiveFlowRow,
} from '../types'

const BASE = '/admin/Archive'

export interface PaginatedResponse<T> {
  items: T[]
  totalCount: number
  pageIndex: number
  pageSize: number
}

export interface BundleListParams {
  pageIndex?: number
  pageSize?: number
  branchId?: number
  stage?: number
  year?: number
  month?: number
  pendingOnly?: boolean
}

export function fetchBundles(params: BundleListParams, signal?: AbortSignal) {
  return axiosInstance
    .get<PaginatedResponse<ArchiveBundleListItem>>(`${BASE}/bundles`, { params, signal })
    .then((r) => r.data)
}

export function fetchBundle(id: number, stage?: number, signal?: AbortSignal) {
  return axiosInstance
    .get<ArchiveBundleDetail>(`${BASE}/bundles/${id}`, { params: { stage }, signal })
    .then((r) => r.data)
}

export function fetchFlowBoard(branchId?: number, signal?: AbortSignal) {
  return axiosInstance
    .get<ArchiveFlowRow[]>(`${BASE}/flow-board`, { params: { branchId }, signal })
    .then((r) => r.data)
}

export function sendBundle(bundleId: number, fromStage: number, note?: string) {
  return axiosInstance
    .post<ArchiveActionResult>(`${BASE}/bundles/${bundleId}/send`, { fromStage, note })
    .then((r) => r.data)
}

export function receiveTransactions(transactionIds: number[], atStage: number, note?: string) {
  return axiosInstance
    .post<ArchiveActionResult>(`${BASE}/transactions/receive`, { transactionIds, atStage, note })
    .then((r) => r.data)
}

export function undoSend(transactionIds: number[], fromStage: number, note?: string) {
  return axiosInstance
    .post<ArchiveActionResult>(`${BASE}/transactions/undo-send`, { transactionIds, fromStage, note })
    .then((r) => r.data)
}

export function returnTransactions(
  transactionIds: number[],
  destination: number,
  reason: string,
) {
  return axiosInstance
    .post<ArchiveActionResult>(`${BASE}/transactions/return`, {
      transactionIds,
      destination,
      reason,
    })
    .then((r) => r.data)
}

export function reconcile() {
  return axiosInstance.post(`${BASE}/reconcile`).then((r) => r.data)
}
