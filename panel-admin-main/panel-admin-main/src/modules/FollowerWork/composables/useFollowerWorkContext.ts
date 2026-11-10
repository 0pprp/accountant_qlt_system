import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { FollowerWorkSort } from '../domain/types'
import { FOLLOWER_WORK_ROUTE_NAME } from '../domain/sections'

const SORTS = new Set<FollowerWorkSort>(['name', 'rating', 'lists'])

function parsePositiveInt(raw: unknown): number | null {
  if (raw == null || raw === '') return null
  const n = Number(raw)
  return Number.isInteger(n) && n > 0 ? n : null
}

function parseSort(raw: unknown): FollowerWorkSort {
  const v = String(raw ?? 'name') as FollowerWorkSort
  return SORTS.has(v) ? v : 'name'
}

function parsePage(raw: unknown): number {
  const n = Number(raw)
  return Number.isInteger(n) && n > 0 ? n : 1
}

export function useFollowerWorkContext() {
  const router = useRouter()
  const route = useRoute()

  const ctx = computed(() => ({
    branchId: parsePositiveInt(route.query.branchId),
    followerId: parsePositiveInt(route.query.followerId),
    search: String(route.query.search ?? '').trim(),
    page: parsePage(route.query.page),
    sort: parseSort(route.query.sort),
  }))

  function patch(
    next: Partial<{
      branchId: number | null
      followerId: number | null
      search: string
      page: number
      sort: FollowerWorkSort
    }>
  ) {
    const cur = ctx.value
    const branchId = next.branchId === undefined ? cur.branchId : next.branchId
    const query: Record<string, string> = {}
    if (branchId != null) query.branchId = String(branchId)

    const followerId =
      next.followerId === undefined ? cur.followerId : next.followerId
    if (followerId != null) query.followerId = String(followerId)

    const search = next.search === undefined ? cur.search : next.search
    if (search) query.search = search

    const page = next.page === undefined ? cur.page : next.page
    if (page > 1) query.page = String(page)

    const sort = next.sort === undefined ? cur.sort : next.sort
    if (sort !== 'name') query.sort = sort

    void router.replace({ name: FOLLOWER_WORK_ROUTE_NAME, query })
  }

  function clearFollower() {
    patch({ followerId: null })
  }

  function setBranch(branchId: number | null) {
    // تغيير المحافظة يعيد تقييم المتابع — لا يستمر اختيار متابع من فرع آخر
    patch({
      branchId,
      followerId: null,
      page: 1,
    })
  }

  return { ctx, patch, clearFollower, setBranch }
}
