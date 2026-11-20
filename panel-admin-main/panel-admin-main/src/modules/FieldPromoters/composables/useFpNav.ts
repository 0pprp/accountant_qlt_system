import { useRoute, useRouter, type LocationQueryRaw } from 'vue-router'
import { useDelegateBranchId } from '@/modules/DelegateManager/composables/useDelegateSummary'

export function useFpNav() {
  const route = useRoute()
  const router = useRouter()
  const branchId = useDelegateBranchId()

  function baseQuery(): LocationQueryRaw {
    return branchId.value == null ? {} : { branchId: String(branchId.value) }
  }

  function openDay(userId: number, date?: string) {
    void router.push({
      name: 'ManagementFieldPromoterDayRoute',
      query: { ...baseQuery(), userId: String(userId), ...(date ? { date } : {}) },
    })
  }

  function replaceQuery(patch: Record<string, string | undefined>) {
    const query: LocationQueryRaw = { ...route.query }
    for (const [k, v] of Object.entries(patch)) {
      if (v == null || v === '') delete query[k]
      else query[k] = v
    }
    void router.replace({ name: route.name ?? undefined, params: route.params, query })
  }

  return { route, router, branchId, openDay, replaceQuery }
}
