import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type {
  ManagementSummaryMetricKey,
  SummaryFiltersState,
} from '../domain/types'

const METRICS = new Set<ManagementSummaryMetricKey>([
  'receipts',
  'expenses',
  'salePrice',
  'purchasePrice',
  'buys',
])

function baghdadNow(): Date {
  return new Date(
    new Date().toLocaleString('en-US', { timeZone: 'Asia/Baghdad' })
  )
}

function parseMetric(raw: unknown): ManagementSummaryMetricKey {
  const v = String(raw ?? '').trim()
  return METRICS.has(v as ManagementSummaryMetricKey)
    ? (v as ManagementSummaryMetricKey)
    : 'receipts'
}

function parseYear(raw: unknown, fallback: number): number {
  const n = Number(raw)
  if (!Number.isInteger(n) || n < 2000 || n > 2100) return fallback
  return n
}

function parseMonth(raw: unknown, fallback: number): number {
  const n = Number(raw)
  if (!Number.isInteger(n) || n < 1 || n > 12) return fallback
  return n
}

function parseBranchId(raw: unknown): number | null {
  if (raw == null || raw === '' || raw === 'all') return null
  const n = Number(raw)
  if (!Number.isInteger(n) || n <= 0) return null
  return n
}

export function useSummaryFilters() {
  const route = useRoute()
  const router = useRouter()
  const now = baghdadNow()
  const defaultYear = now.getFullYear()
  const defaultMonth = now.getMonth() + 1

  const filters = computed<SummaryFiltersState>(() => ({
    year: parseYear(route.query.year, defaultYear),
    month: parseMonth(route.query.month, defaultMonth),
    metric: parseMetric(route.query.metric),
    branchId: parseBranchId(route.query.branchId),
  }))

  function patch(next: Partial<SummaryFiltersState>) {
    const current = filters.value
    const year = next.year ?? current.year
    const month = next.month ?? current.month
    const metric = next.metric ?? current.metric
    const branchId =
      next.branchId === undefined ? current.branchId : next.branchId

    const query: Record<string, string> = {
      year: String(year),
      month: String(month),
      metric,
    }
    if (branchId != null) query.branchId = String(branchId)

    void router.replace({ query })
  }

  watch(
    () => route.query,
    () => {
      const f = filters.value
      const q = route.query
      const needs =
        String(q.year ?? '') !== String(f.year) ||
        String(q.month ?? '') !== String(f.month) ||
        String(q.metric ?? '') !== f.metric ||
        (f.branchId == null
          ? q.branchId != null && q.branchId !== ''
          : String(q.branchId) !== String(f.branchId))
      if (needs) patch(f)
    },
    { immediate: true }
  )

  return { filters, patch }
}
