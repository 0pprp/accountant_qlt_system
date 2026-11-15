import type { Ref } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import { axiosInstance } from '@/modules/Core/plugins/axios'
import type { MatchCriterion, MatchHit } from '@/modules/SalesRequests/types'

export const MATCH_HITS_KEY = 'sales-requests-match-hits'

interface Paginated<T> {
  items: T[]
  totalCount: number
}

function pick<T>(row: Record<string, unknown>, camel: string, pascal: string): T | undefined {
  return (row[camel] ?? row[pascal]) as T | undefined
}

/** يوحّد شكل عناصر المطابقة حتى لو رجعت PascalCase من السيرفر. */
export function normalizeMatchHit(raw: unknown): MatchHit {
  const row = (raw ?? {}) as Record<string, unknown>
  const highlights = pick<unknown>(row, 'highlightedParts', 'HighlightedParts')
  return {
    customerId: Number(pick(row, 'customerId', 'CustomerId') ?? 0),
    customerName: String(pick(row, 'customerName', 'CustomerName') ?? ''),
    matchReason: String(pick(row, 'matchReason', 'MatchReason') ?? ''),
    highlightedParts: Array.isArray(highlights)
      ? highlights.map(String)
      : [],
    phone: (pick(row, 'phone', 'Phone') as string | null | undefined) ?? null,
    branchName:
      (pick(row, 'branchName', 'BranchName') as string | null | undefined) ?? null,
    ratingLabel:
      (pick(row, 'ratingLabel', 'RatingLabel') as string | null | undefined) ?? null,
    score: (pick(row, 'score', 'Score') as number | null | undefined) ?? null,
    isLegal: (pick(row, 'isLegal', 'IsLegal') as boolean | null | undefined) ?? null,
    sellTotal: (pick(row, 'sellTotal', 'SellTotal') as number | null | undefined) ?? null,
    paidTotal: (pick(row, 'paidTotal', 'PaidTotal') as number | null | undefined) ?? null,
    remaining: (pick(row, 'remaining', 'Remaining') as number | null | undefined) ?? null,
    paymentCount:
      (pick(row, 'paymentCount', 'PaymentCount') as number | null | undefined) ?? null,
    paymentDays:
      (pick(row, 'paymentDays', 'PaymentDays') as number | null | undefined) ?? null,
    saleDate: (pick(row, 'saleDate', 'SaleDate') as string | null | undefined) ?? null,
    canOpenProfile: Boolean(pick(row, 'canOpenProfile', 'CanOpenProfile')),
  }
}

export function fetchMatchHits(
  requestId: number,
  criterion: MatchCriterion,
  pageIndex: number,
  pageSize: number
) {
  return axiosInstance
    .get<Paginated<Record<string, unknown>>>(
      `/admin/SalesRequests/${requestId}/match-hits`,
      { params: { criterion, pageIndex, pageSize } }
    )
    .then((r) => {
      const data = r.data ?? { items: [], totalCount: 0 }
      const itemsRaw = (data as { items?: unknown[]; Items?: unknown[] }).items
        ?? (data as { Items?: unknown[] }).Items
        ?? []
      const total =
        (data as { totalCount?: number; TotalCount?: number }).totalCount
        ?? (data as { TotalCount?: number }).TotalCount
        ?? itemsRaw.length
      return {
        items: itemsRaw.map(normalizeMatchHit),
        totalCount: Number(total) || 0,
      } satisfies Paginated<MatchHit>
    })
}

export function useMatchHitsQuery(
  requestId: Ref<number | null>,
  criterion: Ref<MatchCriterion | null>,
  pageIndex: Ref<number>,
  pageSize: Ref<number>,
  enabled: Ref<boolean>
) {
  return useQuery({
    queryKey: [MATCH_HITS_KEY, requestId, criterion, pageIndex, pageSize],
    queryFn: () =>
      fetchMatchHits(
        requestId.value!,
        criterion.value!,
        pageIndex.value,
        pageSize.value
      ),
    enabled,
  })
}
