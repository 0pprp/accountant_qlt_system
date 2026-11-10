import { FOLLOWER_WORK_CANONICAL_PATH } from '../domain/sections'

const ALLOWED_PREFIXES = [
  FOLLOWER_WORK_CANONICAL_PATH,
  '/management/oversight/follower-work',
  '/management/director/follower-work',
] as const

/**
 * returnTo داخلي فقط — يرفض URL خارجيًا ومسارًا غير مسموح.
 */
export function sanitizeFollowerReturnTo(raw: unknown): string | null {
  if (typeof raw !== 'string') return null
  const value = raw.trim()
  if (!value.startsWith('/') || value.startsWith('//')) return null
  if (value.includes('://') || value.includes('\\')) return null
  const path = value.split('?')[0] ?? value
  if (ALLOWED_PREFIXES.some((p) => path === p || path.startsWith(`${p}/`))) {
    return value
  }
  return null
}

export function buildFollowerReturnTo(query: Record<string, string>): string {
  const qs = new URLSearchParams(query).toString()
  return qs
    ? `${FOLLOWER_WORK_CANONICAL_PATH}?${qs}`
    : FOLLOWER_WORK_CANONICAL_PATH
}
