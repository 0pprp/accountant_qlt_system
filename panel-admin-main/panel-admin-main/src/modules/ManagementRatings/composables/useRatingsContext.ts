import { computed, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { RatingsScope } from '../domain/types'

const SCOPES = new Set<RatingsScope>(['branch', 'list', 'follower', 'employee'])

function baghdadTodayIso() {
  const d = new Date(
    new Date().toLocaleString('en-US', { timeZone: 'Asia/Baghdad' })
  )
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

function monthStartIso() {
  const d = new Date(
    new Date().toLocaleString('en-US', { timeZone: 'Asia/Baghdad' })
  )
  const pad = (n: number) => String(n).padStart(2, '0')
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-01`
}

function parseBranchId(raw: unknown): number | null {
  if (raw == null || raw === '') return null
  const n = Number(raw)
  return Number.isInteger(n) && n > 0 ? n : null
}

function parseScope(raw: unknown): RatingsScope {
  const v = String(raw ?? 'branch') as RatingsScope
  return SCOPES.has(v) ? v : 'branch'
}

function parseResourceId(raw: unknown): number {
  const n = Number(raw)
  return Number.isInteger(n) && n > 0 ? n : 0
}

function parseDate(raw: unknown, fallback: string): string {
  const s = String(raw ?? '')
  return /^\d{4}-\d{2}-\d{2}$/.test(s) ? s : fallback
}

export function useRatingsContext() {
  const route = useRouter()
  const r = useRoute()
  const defaultFrom = monthStartIso()
  const defaultTo = baghdadTodayIso()

  const ctx = computed(() => ({
    branchId: parseBranchId(r.query.branchId),
    scope: parseScope(r.query.scope),
    resourceId: parseResourceId(r.query.resourceId),
    from: parseDate(r.query.from, defaultFrom),
    to: parseDate(r.query.to, defaultTo),
    level: String(r.query.level ?? '').trim() || null,
  }))

  function patch(next: Partial<{
    branchId: number | null
    scope: RatingsScope
    resourceId: number
    from: string
    to: string
    level: string | null
  }>) {
    const cur = ctx.value
    const branchId = next.branchId === undefined ? cur.branchId : next.branchId
    const query: Record<string, string> = {}
    if (branchId != null) {
      query.branchId = String(branchId)
      query.scope = next.scope ?? cur.scope
      const resourceId = next.resourceId === undefined ? cur.resourceId : next.resourceId
      if (resourceId > 0) query.resourceId = String(resourceId)
      query.from = next.from ?? cur.from
      query.to = next.to ?? cur.to
      const level = next.level === undefined ? cur.level : next.level
      if (level) query.level = level
    }
    void route.replace({ name: 'ManagementRatingsRoute', query })
  }

  watch(
    () => r.query,
    () => {
      /* validated reads happen via computed */
    }
  )

  return { ctx, patch, clearBranch: () => patch({ branchId: null }) }
}
