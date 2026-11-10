import type { IntendedNavigation } from './session.types'
import type { RouteLocationNormalized } from 'vue-router'

let intended: IntendedNavigation | null = null

export function rememberIntendedNavigation(
  to: Pick<RouteLocationNormalized, 'name' | 'fullPath' | 'params' | 'query'>
) {
  intended = {
    name: to.name as string | null | undefined,
    fullPath: to.fullPath,
    params: { ...(to.params as Record<string, unknown>) },
    query: { ...(to.query as Record<string, unknown>) },
  }
}

export function consumeIntendedNavigation(): IntendedNavigation | null {
  const value = intended
  intended = null
  return value
}

export function peekIntendedNavigation(): IntendedNavigation | null {
  return intended
}

export function clearIntendedNavigation() {
  intended = null
}
