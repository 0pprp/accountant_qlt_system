import { beforeEach, describe, expect, it, vi } from 'vitest'
import { reactive } from 'vue'

const replace = vi.fn()
const route = reactive<{
  name: string | undefined
  params: Record<string, string>
  query: Record<string, string>
}>({ name: undefined, params: {}, query: {} })

vi.mock('vue-router', () => ({
  useRouter: () => ({ replace }),
  useRoute: () => route,
}))

import { RATINGS_FALLBACK_ROUTE, useRatingsContext } from '../useRatingsContext'

function setRoute(name: string | undefined, query: Record<string, string> = {}, params: Record<string, string> = {}) {
  route.name = name
  route.query = query
  route.params = params
}

describe('useRatingsContext', () => {
  beforeEach(() => {
    replace.mockReset()
    setRoute(undefined)
  })

  it('keeps the delegate route name when patching', () => {
    setRoute('ManagementDelegateRatingsRoute', { branchId: '3' })
    const { patch } = useRatingsContext()
    patch({ scope: 'list', resourceId: 9 })
    expect(replace).toHaveBeenCalledTimes(1)
    const target = replace.mock.calls[0][0]
    expect(target.name).toBe('ManagementDelegateRatingsRoute')
    expect(target.query).toMatchObject({ branchId: '3', scope: 'list', resourceId: '9' })
  })

  it('keeps the branch route and its params', () => {
    setRoute('ManagementBranchRatingsRoute', { branchId: '5' }, { id: '7' })
    const { patch } = useRatingsContext()
    patch({ level: 'good' })
    const target = replace.mock.calls[0][0]
    expect(target.name).toBe('ManagementBranchRatingsRoute')
    expect(target.params).toEqual({ id: '7' })
    expect(target.query.level).toBe('good')
  })

  it('clearing the branch stays on the current route', () => {
    setRoute('ManagementDelegateRatingsRoute', { branchId: '3', scope: 'follower', resourceId: '4' })
    const { clearBranch } = useRatingsContext()
    clearBranch()
    const target = replace.mock.calls[0][0]
    expect(target.name).toBe('ManagementDelegateRatingsRoute')
    expect(target.query).toEqual({})
  })

  it('falls back to the oversight route when the current route has no name', () => {
    setRoute(undefined, { branchId: '2' })
    const { patch, routeName } = useRatingsContext()
    expect(routeName.value).toBe(RATINGS_FALLBACK_ROUTE)
    patch({ scope: 'branch' })
    expect(replace.mock.calls[0][0].name).toBe(RATINGS_FALLBACK_ROUTE)
  })

  it('keeps the drilled list only for follower scope', () => {
    setRoute('ManagementDelegateRatingsRoute', { branchId: '3', scope: 'follower', resourceId: '4' })
    const { patch, ctx } = useRatingsContext()
    patch({ drill: 11 })
    expect(replace.mock.calls[0][0].query.drill).toBe('11')

    setRoute('ManagementDelegateRatingsRoute', { branchId: '3', scope: 'list', resourceId: '4', drill: '11' })
    expect(ctx.value.drill).toBe(0)
    patch({ drill: 11 })
    expect(replace.mock.calls[1][0].query.drill).toBeUndefined()
  })

  it('defaults the window to the first of the month through today', () => {
    setRoute('ManagementDelegateRatingsRoute', { branchId: '3' })
    const { ctx } = useRatingsContext()
    expect(ctx.value.from).toMatch(/^\d{4}-\d{2}-01$/)
    expect(ctx.value.to >= ctx.value.from).toBe(true)
    expect(ctx.value.to.slice(0, 7)).toBe(ctx.value.from.slice(0, 7))
  })
})
