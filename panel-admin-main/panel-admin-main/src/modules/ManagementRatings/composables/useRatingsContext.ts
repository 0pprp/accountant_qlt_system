import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { RatingsScope } from '../domain/types'

const SCOPES = new Set<RatingsScope>(['branch', 'list', 'follower', 'employee'])

export const RATINGS_FALLBACK_ROUTE = 'ManagementRatingsRoute'

function baghdadNow() {
  return new Date(new Date().toLocaleString('en-US', { timeZone: 'Asia/Baghdad' }))
}

function pad(n: number) {
  return String(n).padStart(2, '0')
}

export function baghdadTodayIso() {
  const d = baghdadNow()
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
}

export function monthStartIso() {
  const d = baghdadNow()
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-01`
}

function first(raw: unknown): unknown {
  return Array.isArray(raw) ? raw[0] : raw
}

function parseBranchId(raw: unknown): number | null {
  const v = first(raw)
  if (v == null || v === '') return null
  const n = Number(v)
  return Number.isInteger(n) && n > 0 ? n : null
}

function parseScope(raw: unknown): RatingsScope {
  const v = String(first(raw) ?? 'branch') as RatingsScope
  return SCOPES.has(v) ? v : 'branch'
}

function parseResourceId(raw: unknown): number {
  const n = Number(first(raw))
  return Number.isInteger(n) && n > 0 ? n : 0
}

function parseDate(raw: unknown, fallback: string): string {
  const s = String(first(raw) ?? '')
  return /^\d{4}-\d{2}-\d{2}$/.test(s) ? s : fallback
}

export interface RatingsContextPatch {
  branchId: number | null
  scope: RatingsScope
  resourceId: number
  drill: number
  from: string
  to: string
  level: string | null
}

export function useRatingsContext() {
  const router = useRouter()
  const route = useRoute()
  const defaultFrom = monthStartIso()
  const defaultTo = baghdadTodayIso()

  const ctx = computed(() => {
    const scope = parseScope(route.query.scope)
    return {
      branchId: parseBranchId(route.query.branchId),
      scope,
      resourceId: parseResourceId(route.query.resourceId),
      drill: scope === 'follower' ? parseResourceId(route.query.drill) : 0,
      from: parseDate(route.query.from, defaultFrom),
      to: parseDate(route.query.to, defaultTo),
      level: String(first(route.query.level) ?? '').trim() || null,
    }
  })

  const routeName = computed(() =>
    typeof route.name === 'string' && route.name ? route.name : RATINGS_FALLBACK_ROUTE
  )

  function patch(next: Partial<RatingsContextPatch>) {
    const cur = ctx.value
    const branchId = next.branchId === undefined ? cur.branchId : next.branchId
    const query: Record<string, string> = {}
    if (branchId != null) {
      const scope = next.scope ?? cur.scope
      query.branchId = String(branchId)
      query.scope = scope
      const resourceId = next.resourceId === undefined ? cur.resourceId : next.resourceId
      if (resourceId > 0) query.resourceId = String(resourceId)
      const drill = next.drill === undefined ? cur.drill : next.drill
      if (scope === 'follower' && drill > 0) query.drill = String(drill)
      query.from = next.from ?? cur.from
      query.to = next.to ?? cur.to
      const level = next.level === undefined ? cur.level : next.level
      if (level) query.level = level
    }
    void router.replace({ name: routeName.value, params: route.params, query })
  }

  return { ctx, routeName, patch, clearBranch: () => patch({ branchId: null }) }
}
